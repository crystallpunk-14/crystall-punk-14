import json
import os

from . import Prototype, check_prototype_attrs, get_logger, LogText, ErrorWhileWritingToFile, ErrorWhileReadingFromFile
from .parsers import FtlParser, YamlParser, create_ftl

logger = get_logger(__name__)

SAVE_RESULT_TO = "entities.ftl"
YAML_PARSER_LAST_LAUNCH_RESULT_PATH = "last_launch_result/result.json"


class LocalizationHelper:

    def __init__(self):
        logger.debug("%s LocalizationHelper ", LogText.CLASS_INITIALIZATION)

    @staticmethod
    def _save_to_json(path: str, data: dict):
        os.makedirs(os.path.dirname(path), exist_ok=True)
        try:
            logger.debug("%s: %s", LogText.SAVING_DATA_TO_FILE, path)
            with open(path, "w", encoding="utf-8") as file:
                json.dump(data, file, ensure_ascii=False, indent=4)
        except Exception as e:
            raise ErrorWhileWritingToFile(e)

    @staticmethod
    def _read_from_json(path: str) -> dict:
        if os.path.exists(path):
            try:
                logger.debug("%s: %s", LogText.READING_DATA_FROM_FILE, path)
                with open(path, encoding="utf-8") as file:
                    return json.load(file)
            except Exception as e:
                raise ErrorWhileReadingFromFile(e)
        return {}

    def _save_yaml_parser_last_launch_result(self, last_launch_result: dict[str, Prototype]):
        logger.debug("%s %s", LogText.SAVING_LAST_LAUNCH_RESULT, YAML_PARSER_LAST_LAUNCH_RESULT_PATH)

        prototypes_dict = {}
        for prototype_id, prototype_obj in last_launch_result.items():
            prototypes_dict[prototype_id] = prototype_obj.attrs_dict

        self._save_to_json(YAML_PARSER_LAST_LAUNCH_RESULT_PATH, prototypes_dict)

    def _read_prototypes_from_last_launch_result(self) -> dict[str, Prototype] | None:
        if os.path.isfile(YAML_PARSER_LAST_LAUNCH_RESULT_PATH):
            last_launch_result = self._read_from_json(YAML_PARSER_LAST_LAUNCH_RESULT_PATH)
            last_launch_result_dict = {}
            for prototype_id, prototype_attrs in last_launch_result.items():
                last_launch_result_dict[prototype_id] = Prototype(prototype_attrs)

            return last_launch_result_dict
        return None

    @staticmethod
    def _update_prototype_if_attrs_has_been_changed(yaml_prototype_obj: Prototype, last_launch_prototype_obj: Prototype,
                                                    final_prototype_obj: Prototype):
        if yaml_prototype_obj.attrs_dict != last_launch_prototype_obj.attrs_dict:
            log_text = f"Has been updated from: {final_prototype_obj.attrs_dict}, to: "

            for key, value in yaml_prototype_obj.attrs_dict.items():
                if final_prototype_obj.attrs_dict[key] != value:
                    final_prototype_obj.set_attrs_dict_value(key, value)

            log_text += f"{final_prototype_obj.attrs_dict}"
            logger.debug(log_text)

        return final_prototype_obj

    def _merge_yaml_parser_prototypes_and_ftl_parser_prototypes(self, yaml_parser_prototypes: dict[str, Prototype],
                                                                ftl_parser_prototypes: dict[str, Prototype]) -> dict[str, Prototype]:

        general_prototypes_dict = {}

        last_launch_result = self._read_prototypes_from_last_launch_result()
        for prototype_id, yaml_prototype_obj in yaml_parser_prototypes.items():
            final_prototype_obj = yaml_prototype_obj
            if prototype_id in ftl_parser_prototypes:
                final_prototype_obj = ftl_parser_prototypes[prototype_id]

            if last_launch_result and prototype_id in last_launch_result:
                last_launch_prototype_obj = last_launch_result[prototype_id]
                final_prototype_obj = self._update_prototype_if_attrs_has_been_changed(yaml_prototype_obj,
                                                                                       last_launch_prototype_obj,
                                                                                       final_prototype_obj)
            general_prototypes_dict[prototype_id] = final_prototype_obj
        return general_prototypes_dict

    @staticmethod
    def _set_parent_attrs(prototype_parent_id: str, prototype_obj: Prototype, parent_prototype_obj: Prototype):
        for attr_name, attr_value in prototype_obj.attrs_dict.items():
            if attr_value or attr_name in ("parent", "id"):
                continue

            parent_prototype_attr_value = parent_prototype_obj.attrs_dict.get(attr_name)
            if parent_prototype_attr_value:
                if attr_name == "name":
                    prototype_obj.name = f"{{ ent-{prototype_parent_id} }}"
                elif attr_name == "description":
                    prototype_obj.description = f"{{ ent-{prototype_parent_id}.desc }}"
                elif attr_name == "suffix":
                    prototype_obj.suffix = parent_prototype_attr_value

        return prototype_obj

    def _parent_checks(self, general_prototypes_dict: dict[str, Prototype]):
        to_delete = []
        for prototype_id, prototype_obj in general_prototypes_dict.items():
            prototype_parent_id = prototype_obj.parent
            if isinstance(prototype_parent_id, list):
                continue

            parent_prototype_obj = general_prototypes_dict.get(prototype_parent_id)

            if parent_prototype_obj and check_prototype_attrs(parent_prototype_obj, False):
                self._set_parent_attrs(prototype_parent_id, prototype_obj, parent_prototype_obj)
            else:
                if not check_prototype_attrs(prototype_obj, False):
                    to_delete.append(prototype_id)

        for prototype_id in to_delete:
            logger.debug("%s %s: %s", prototype_id, LogText.HAS_BEEN_DELETED, general_prototypes_dict[prototype_id])
            del general_prototypes_dict[prototype_id]

        return general_prototypes_dict

    def _create_general_prototypes_dict(self, yaml_parser_prototypes: dict[str, Prototype],
                                        ftl_parser_prototypes: dict[str, Prototype]) -> dict[str, Prototype]:

        general_prototypes_dict = self._merge_yaml_parser_prototypes_and_ftl_parser_prototypes(yaml_parser_prototypes,
                                                                                               ftl_parser_prototypes)

        self._save_yaml_parser_last_launch_result(yaml_parser_prototypes)
        general_prototypes_dict = self._parent_checks(general_prototypes_dict)
        return general_prototypes_dict

    @staticmethod
    def _create_result_ftl(general_prototypes_dict: dict[str, Prototype]) -> str:
        result = ""
        for prototype_obj in general_prototypes_dict.values():
            result += create_ftl(prototype_obj)
        return result

    def _save_result(self, general_prototypes_dict: dict[str, Prototype]):
        logger.debug("%s: %s", LogText.SAVING_FINAL_RESULT, SAVE_RESULT_TO)
        result = self._create_result_ftl(general_prototypes_dict)
        try:
            with open(SAVE_RESULT_TO, "w", encoding="utf-8") as file:
                file.write(result)
        except Exception as e:
            raise ErrorWhileWritingToFile(e)

    @staticmethod
    def _print_info(general_prototypes_dict):
        logger.info("%s: %s prototypes", LogText.HAS_BEEN_PROCESSED, len(general_prototypes_dict))
        logger.info("Logs in: 'logs/helper.log'")

    def main(self, yaml_prototypes_path: str, ftl_prototypes_path: str):
        try:
            logger.debug("%s: %s", LogText.GETTING_PROTOTYPES_FROM_YAML, yaml_prototypes_path)
            prototypes_list_parsed_from_yaml = YamlParser().get_prototypes(yaml_prototypes_path)

            logger.debug("%s: %s", LogText.GETTING_PROTOTYPES_FROM_FTL, ftl_prototypes_path)
            prototypes_list_parsed_from_ftl = FtlParser().get_prototypes(ftl_prototypes_path)

            logger.debug(LogText.FORMING_FINAL_DICTIONARY)
            general_prototypes_dict = self._create_general_prototypes_dict(prototypes_list_parsed_from_yaml,
                                                                           prototypes_list_parsed_from_ftl)

            self._save_result(general_prototypes_dict)
            self._print_info(general_prototypes_dict)
        except ErrorWhileWritingToFile as e:
            logger.error("%s: %s", LogText.ERROR_WHILE_WRITING_DATA_TO_FILE, e, exc_info=True)
        except ErrorWhileReadingFromFile as e:
            logger.error("%s: %s", LogText.ERROR_WHILE_READING_DATA_FROM_FILE, e, exc_info=True)
        except Exception as e:
            logger.error("%s: %s", LogText.UNKNOWN_ERROR, e, exc_info=True)
