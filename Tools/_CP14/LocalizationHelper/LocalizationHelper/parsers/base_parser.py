import os
from abc import ABC, abstractmethod


from LocalizationHelper.entity import Entity


class BaseParser(ABC):
    @staticmethod
    def _get_files_paths_in_dir(path: str) -> list[str]:
        files_paths_lst = []

        for dirpath, _, filenames in os.walk(path):
            for filename in filenames:
                file_path = f"{dirpath}\\{filename}"
                files_paths_lst.append(file_path)

        return files_paths_lst

    @staticmethod
    def _check_file_extension(path: str, extension: str) -> bool:
        if path.endswith(extension):
            return True
        return False

    @abstractmethod
    def get_prototypes(self, prototypes_files_path: str) -> list[Entity]:
        pass
