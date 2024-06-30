import os
import json


class BaseParser:
    """
    BaseParser, contains the basic functions for the yml_parser module in the yml_parser package
    and for the ftl_parser module in the ftl_parser package
    """
    def __init__(self, paths: tuple):
        self.path, self.errors_path = paths

    def get_files_paths(self) -> list:
        """
        The method gets the path to the yml folder of localization prototypes/files, e.g. "ftl",
        then with the help of os library goes through each file in
        the folder and creates a path for it, e.g. "ftl/objects.ftl".
        """
        files_paths_lst = []

        for dirpath, _, filenames in os.walk(self.path):
            for filename in filenames:
                path = f"{dirpath}\\{filename}"
                files_paths_lst.append(path)

        return files_paths_lst

    @staticmethod
    def save_to_json(prototypes: dict, path: str) -> None:
        with open(path, 'w') as json_file:
            json.dump(prototypes, json_file, indent=4)

    @staticmethod
    def check_file_extension(path: str, extension: str) -> bool:
        if path.endswith(extension):
            return True
        return False

