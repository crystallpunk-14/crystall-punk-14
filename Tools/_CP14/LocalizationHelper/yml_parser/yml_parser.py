import yaml
from base_parser import BaseParser
import re


class YMLParser(BaseParser):
    """
    The class inherits from the "BaseParser" class, parses yml prototypes.
    """

    @staticmethod
    def _check_proto_attrs(prototype: dict) -> bool:
        """
        The function checks that the prototype at least has some attribute from the "attrs_lst".
        """
        attrs_lst = ["name", "description", "suffix"]
        # In some cases a parent can be a list (because of multiple parents),
        # the game will not be able to handle such cases in ftl files.
        if not isinstance(prototype.get("parent"), list):
            attrs_lst.append("parent")

        return any(prototype.get(attr) is not None for attr in attrs_lst)

    @staticmethod
    def _get_proto_attrs(prototypes: dict, prototype: dict) -> None:
        prototypes[prototype.get("id")] = {
            "parent": prototype.get("parent"),
            "name": prototype.get("name"),
            "desc": prototype.get("description"),
            "suffix": prototype.get("suffix")
        }

    def _load_proto(self, file, path) -> list[dict]:
        content_str = file.read()
        prototypes_lst = re.split(r"\n(?=- type:)", content_str)

        prototypes = []
        for proto in prototypes_lst:
            try:
                prototype_str = ""
                for line in proto.splitlines():
                    if "components:" in line:
                        break
                    prototype_str += f"{line}\n"
                prototype = yaml.safe_load(prototype_str)
                if prototype is None:
                    continue
                prototypes.append(prototype[0])
            except Exception as e:
                with open(self.errors_path, "a") as error_file:
                    error_file.write(
                        f"YML-ERROR:\nAn error occurred during prototype processing {path}, error - {e}\n")

        return prototypes
        
    def yml_parser(self) -> dict:
        """
            The function gets the path, then with the help of the os library
            goes through each file,checks that the file extension is "yml",
            then processes the file using the "PyYaml" library
        """
        prototypes = {}

        for path in self._get_files_paths():
            if not self._check_file_extension(path, ".yml"):
                continue

            with open(path, encoding="utf-8") as file:
                content = self._load_proto(file, path)

            if content is not None:
                for prototype in content:
                    if self._check_proto_attrs(prototype):
                        self._get_proto_attrs(prototypes, prototype)

        return prototypes
