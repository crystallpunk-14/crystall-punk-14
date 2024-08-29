import json
import os
from yml_parser import YMLParser
from ftl_parser import FTLParser
from fluent import ftl_writer

# Config constants
CONFIG_PATH = "config.json"
CONFIG_PATHS_KEY_NAME = "paths"
PROTOTYPES_PATH_IN_CONFIG = "prototypes"
FTL_PATH_IN_CONFIG = "localization"
ERRORS_LOG_PATH_IN_CONFIG = "error_log_path"
PARSED_PROTOTYPES_PATH_IN_LAST_LAUNCH = "yml_parser_last_launch"

LAST_LAUNCH_PROTOTYPES_DIR_NAME = "last_launch"
NAME_OF_FILE_TO_SAVE = "entities.ftl"


class LocalizationHelper:

    if not os.path.isdir(LAST_LAUNCH_PROTOTYPES_DIR_NAME):
        os.mkdir(LAST_LAUNCH_PROTOTYPES_DIR_NAME)

    def __init__(self, config_path: str):
        self._config = self._read_config(config_path)
        self._prototypes_path, self._localization_path, self._errors_log_path, self._yml_parser_last_launch = self._get_paths()
        self._clear_logs()
        self.prototypes_dict_yml = YMLParser((self._prototypes_path, self._errors_log_path)).yml_parser()
        self.prototypes_dict_ftl = FTLParser((self._localization_path, self._errors_log_path)).ftl_parser()
        self._check_changed_attrs()
        self.prototypes = {**self.prototypes_dict_yml, **self.prototypes_dict_ftl}

    def _clear_logs(self):
        with open(self._errors_log_path, "w") as file:
            file.write("")
    
    @staticmethod
    def _read_config(config_path: str) -> dict:
        with open(config_path, "r", encoding="utf-8") as file:
            return json.load(file)

    def _get_paths(self) -> tuple:
        paths_dict = self._config[CONFIG_PATHS_KEY_NAME]
        prototypes_path = paths_dict[PROTOTYPES_PATH_IN_CONFIG]
        localization_path = paths_dict[FTL_PATH_IN_CONFIG]
        errors_log_path = paths_dict[ERRORS_LOG_PATH_IN_CONFIG]
        yml_parser_last_launch = paths_dict[PARSED_PROTOTYPES_PATH_IN_LAST_LAUNCH]
        return prototypes_path, localization_path, errors_log_path, yml_parser_last_launch

    def _check_changed_attrs(self):
        """

        What error it fixes - without this function, changed attributes of prototypes that have not been changed in
        localization files will simply not be added to the original ftl file, because the script first of all takes data
        from localization files, if they exist, of course

        The function gets the data received during the last run of the script, and checks if some attribute from
        the last run has been changed,then simply replaces with this attribute the attribute
        of the prototype received during parsing of localization files.
        """
        if os.path.isfile(self._yml_parser_last_launch):
            with open(self._yml_parser_last_launch, 'r', encoding='utf-8') as file:
                last_launch_prototypes = json.load(file)

            if last_launch_prototypes:
                for prototype, last_launch_attrs in last_launch_prototypes.items():
                    if prototype in self.prototypes_dict_yml:
                        if prototype in self.prototypes_dict_ftl:
                            attrs = self.prototypes_dict_ftl[prototype]
                            proto_attrs_in_yml = self.prototypes_dict_yml[prototype]

                            for key, value in proto_attrs_in_yml.items():
                                if value != last_launch_attrs.get(key):
                                    attrs[key] = value

                            self.prototypes_dict_ftl[prototype] = attrs
                    else:
                        if prototype in self.prototypes_dict_ftl:
                            del self.prototypes_dict_ftl[prototype]

    @staticmethod
    def _save_result(entities: str) -> None:
        with open(NAME_OF_FILE_TO_SAVE, "w", encoding="utf-8") as file:
            file.write(entities)

        print(f"{NAME_OF_FILE_TO_SAVE} has been created\n")

    @staticmethod
    def _print_errors_log_info(errors_log_path: str, prototypes: dict) -> None:
        with open(errors_log_path, "r") as file:
            errors = file.read()

        successful_count = len(prototypes) - errors.count("ERROR")
        print(f"""Of the {len(prototypes)} prototypes, {successful_count} were successfully processed.

    Errors can be found in  {errors_log_path}
    Number of errors during YML processing - {errors.count("YML-ERROR")}
    Number of errors during FTL processing - {errors.count("FTL-ERROR")}
    Number of errors during data extraction and creation of new FTL  - {errors.count("RETRIEVING-ERROR")}""")

    def main(self):
        entities_ftl = ""
        for prototype, prototype_attrs in self.prototypes.items():
            try:
                # This fragment is needed to restore some attributes after connecting the dictionary of
                # prototypes parsed from ftl with the dictionary of prototypes parsed from yml.
                if prototype in self.prototypes_dict_yml:
                    parent = self.prototypes_dict_yml[prototype]["parent"]

                    if parent and not isinstance(parent, list) and parent in self.prototypes_dict_yml:
                        if not prototype_attrs.get("name"):
                            prototype_attrs["name"] = f"{{ ent-{parent} }}"

                        if not prototype_attrs.get("desc"):
                            prototype_attrs["desc"] = f"{{ ent-{parent}.desc }}"

                    if not prototype_attrs.get("suffix"):
                        if self.prototypes_dict_yml[prototype].get("suffix"):
                            prototype_attrs["suffix"] = self.prototypes_dict_yml[prototype]["suffix"]

                if any(prototype_attrs[attr] is not None for attr in ("name", "desc", "suffix")):
                    proto_ftl = ftl_writer.create_ftl(prototype, self.prototypes[prototype])
                    entities_ftl += proto_ftl
            except Exception as e:
                with open(self._errors_log_path, "a") as file:
                    print(prototype, prototype_attrs)
                    file.write(
                        f"RETRIEVING-ERROR:\nAn error occurred while retrieving data to be written to the file - {e}\n")

        self._save_result(entities_ftl)
        self._print_errors_log_info(self._errors_log_path, self.prototypes)


if __name__ == '__main__':
    helper = LocalizationHelper(CONFIG_PATH)
    helper.main()
