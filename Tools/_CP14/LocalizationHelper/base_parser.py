import os
from datetime import datetime
import json


class BaseParser:

    def __init__(self, paths: tuple):
        self.path, self.errors_path = paths

    def get_files_paths(self) -> list:
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

    @staticmethod
    def get_last_edit_time(path) -> int:
        modification_time = os.path.getmtime(path)
        modification_datetime = datetime.fromtimestamp(modification_time)
        formatted_time = modification_datetime.strftime('%Y%m%d%H%M%S')

        return int(formatted_time)
