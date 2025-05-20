import json
import os

from . import Entity, check_prototype_attrs, get_logger, LogText, ErrorWhileWritingToFile, ErrorWhileReadingFromFile
from .parsers import FtlParser, YamlParser, create_ftl

logger = get_logger(__name__)

SAVE_RESULT_TO = "entities.ftl"
YAML_PARSER_LAST_LAUNCH_RESULT_PATH = "last_launch_result/result.json"


class LocalizationHelper:

    def __init__(self):
        logger.debug("%s LocalizationHelper ", LogText.CLASS_INITIALIZATION)

    @staticmethod
    def _save_to_json(path: str, data: dict):
        """
        Saves data to a JSON file
        """
        os.makedirs(os.path.dirname(path), exist_ok=True)
        try:
            logger.debug("%s: %s", LogText.SAVING_DATA_TO_FILE, path)
            with open(path, "w", encoding="utf-8") as file:
                json.dump(data, file, ensure_ascii=False, indent=4)
        except Exception as e:
            raise ErrorWhileWritingToFile(e)

    @staticmethod
    def _read_from_json(path: str) -> dict:
        """
        Reads data from a JSON file
        """
        if os.path.exists(path):
            try:
                logger.debug("%s: %s", LogText.READING_DATA_FROM_FILE, path)
                with open(path, encoding="utf-8") as file:
                    return json.load(file)
            except Exception as e:
                raise ErrorWhileReadingFromFile(e)
        return {}

    def _save_yaml_parser_last_launch_result(self, last_launch_result: dict[str, Entity]):
        """
        Updates all prototypes and their attributes at last launch JSON file
        """
        logger.debug("%s %s", LogText.SAVING_LAST_LAUNCH_RESULT, YAML_PARSER_LAST_LAUNCH_RESULT_PATH)

        prototypes_dict = {}
        for prototype_id, prototype_obj in last_launch_result.items():
            prototypes_dict[prototype_id] = prototype_obj.attrs_dict

        self._save_to_json(YAML_PARSER_LAST_LAUNCH_RESULT_PATH, prototypes_dict)

    def _read_prototypes_from_last_launch_result(self) -> dict[str, Entity] | None:
        """
        Reads all prototypes from the last launch JSON file 
        """
        if os.path.isfile(YAML_PARSER_LAST_LAUNCH_RESULT_PATH):
            last_launch_result = self._read_from_json(YAML_PARSER_LAST_LAUNCH_RESULT_PATH)
            last_launch_result_dict = {}
            for prototype_id, prototype_attrs in last_launch_result.items():
                last_launch_result_dict[prototype_id] = Entity(prototype_attrs)

            return last_launch_result_dict
        return None

    @staticmethod
    def _update_prototype_if_attrs_has_been_changed(yaml_prototype_obj: Entity, last_launch_prototype_obj: Entity,
                                                    final_prototype_obj: Entity):
        """
        Updates the prototype if their attributes have changed
        Compares the YAML prototype with the FTL and overwrites if there have been changes
        """
        if yaml_prototype_obj.attrs_dict != last_launch_prototype_obj.attrs_dict:
            log_text = f"Has been updated from: {final_prototype_obj.attrs_dict}, to: "

            for key, value in yaml_prototype_obj.attrs_dict.items():
                if final_prototype_obj.attrs_dict[key] != value:
                    final_prototype_obj.set_attrs_dict_value(key, value)

            log_text += f"{final_prototype_obj.attrs_dict}"
            logger.debug(log_text)

        return final_prototype_obj

    def _merge_yaml_parser_prototypes_and_ftl_parser_prototypes(self, yaml_parser_prototypes: dict[str, Entity],
                                                                ftl_parser_prototypes: dict[str, Entity]) -> dict[str, Entity]:
        """
        Combines YAML and FTL prototypes with persistence of changes
        """
        general_prototypes_dict = {}

        last_launch_result = self._read_prototypes_from_last_launch_result()
        for prototype_id, yaml_prototype_obj in yaml_parser_prototypes.items():
            final_prototype_obj = yaml_prototype_obj
            if prototype_id in ftl_parser_prototypes:
                final_prototype_obj = ftl_parser_prototypes[prototype_id]

            final_prototype_obj.parent = yaml_prototype_obj.parent

            if last_launch_result and prototype_id in last_launch_result:
                last_launch_prototype_obj = last_launch_result[prototype_id]
                final_prototype_obj = self._update_prototype_if_attrs_has_been_changed(yaml_prototype_obj,
                                                                                       last_launch_prototype_obj,
                                                                                       final_prototype_obj)
            general_prototypes_dict[prototype_id] = final_prototype_obj
        return general_prototypes_dict

    @staticmethod
    def _add_parent_attrs(prototype_parent_id: str, prototype_obj: Entity, parent_prototype_obj: Entity):
        """
        Adds parent's attributes to the entity
        """
        for attr_name, attr_value in prototype_obj.attrs_dict.items():
            if attr_value or attr_name in ("parent", "id"):
                continue

            parent_prototype_attr_value = parent_prototype_obj.attrs_dict.get(attr_name)
            if parent_prototype_attr_value:
                if attr_name == "name" and not prototype_obj.name:
                    prototype_obj.name = f"{{ ent-{prototype_parent_id} }}"
                elif attr_name == "description" and not prototype_obj.description:
                    prototype_obj.description = f"{{ ent-{prototype_parent_id}.desc }}"
                elif attr_name == "suffix" and not prototype_obj.suffix:
                    prototype_obj.suffix = parent_prototype_attr_value

        return prototype_obj

    def _add_all_parents_attributes(self, general_prototypes_dict: dict[str, Entity], prototype_id: str, main_prototype_obj: Entity = None):
        '''
        Recursively finds all object parents and adds to his attributes parents attributes
        '''
        prototype_obj = general_prototypes_dict.get(prototype_id)

        if prototype_obj is None: # TODO for asqw: moment when we find wizden parent. We must parse them
            return

        if not main_prototype_obj:
            main_prototype_obj = prototype_obj

        if main_prototype_obj != prototype_obj and check_prototype_attrs(prototype_obj):
            for _ in prototype_obj.attrs_dict.items():
                self._add_parent_attrs(prototype_id, main_prototype_obj, prototype_obj) # TODO for asqw: it is adds from one prototype to another prototype, naming work
        
        if main_prototype_obj.name and main_prototype_obj.description and main_prototype_obj.suffix:
            return

        if prototype_obj.parent:
            prototype_parent_id_list = []

            if isinstance(prototype_obj.parent, list): # Makes id list list if it is not list (TODO for asqw: it must be list at parent writing)
                prototype_parent_id_list.extend(prototype_obj.parent)
            else:
                prototype_parent_id_list.append(prototype_obj.parent)
            
            for prototype_parent_id in prototype_parent_id_list:
                self._add_all_parents_attributes(general_prototypes_dict, prototype_parent_id, main_prototype_obj)
        else:
            return

    def _parent_checks(self, general_prototypes_dict: dict[str, Entity]):
        """
        Adds parent's attributes at all entities in general prototypes dictionary and returns new copy
        """
        to_delete = []
        for prototype_id, prototype_obj in general_prototypes_dict.items():
            if check_prototype_attrs(prototype_obj):
                self._add_all_parents_attributes(general_prototypes_dict, prototype_id)
            else:
                to_delete.append(prototype_id)

        for prototype_id in to_delete:
            logger.debug("%s %s: %s", prototype_id, LogText.HAS_BEEN_DELETED, general_prototypes_dict[prototype_id])
            del general_prototypes_dict[prototype_id] # Deletes prototype if ID wasn't found

        return general_prototypes_dict

    def _create_general_prototypes_dict(self, yaml_parser_prototypes: dict[str, Entity],
                                        ftl_parser_prototypes: dict[str, Entity]) -> dict[str, Entity]:
        """
        Creates a prototype dictionary by combining YAML and FTL
        Preserves YAML parsing data
        Replaces prototype attributes with their parent attributes
        """
        general_prototypes_dict = self._merge_yaml_parser_prototypes_and_ftl_parser_prototypes(yaml_parser_prototypes,
                                                                                               ftl_parser_prototypes)

        self._save_yaml_parser_last_launch_result(yaml_parser_prototypes)
        general_prototypes_dict = self._parent_checks(general_prototypes_dict)
        return general_prototypes_dict

    @staticmethod
    def _create_result_ftl(general_prototypes_dict: dict[str, Entity]) -> str:
        """
        Creates string for FTL writing
        """
        result = ""
        for prototype_obj in general_prototypes_dict.values():
            result += create_ftl(prototype_obj)
        return result

    def _save_result(self, general_prototypes_dict: dict[str, Entity]):
        """
        Saves prototypes to an FTL file
        """
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
