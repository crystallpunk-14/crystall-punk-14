import yaml
from base_parser import BaseParser


class YMLParser(BaseParser):
    """
    The class inherits from the "BaseParser" class, parses yml prototypes.
    """

    @staticmethod
    def check_proto_attrs(prototype: dict) -> bool:
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
    def get_proto_attrs(prototypes: dict, prototype: dict) -> None:
        prototypes[prototype.get("id")] = {
            "parent": prototype.get("parent"),
            "name": prototype.get("name"),
            "desc": prototype.get("description"),
            "suffix": prototype.get("suffix")
        }

    @staticmethod
    def create_proto(file) -> str:
        proto = ""
        for line in file.readlines():
            # The PyYaml library cannot handle the following SpaceStation 14 prototype syntax - !type: ...
            # We need to fix this :(
            if "!type" in line:
                continue
            proto += line

        return proto

    def yml_parser(self) -> dict:
        """
            The function gets the path, then with the help of the os library
            goes through each file,checks that the file extension is "yml",
            then processes the file using the "PyYaml" library
        """
        prototypes = {}

        for path in self.get_files_paths():
            if not self.check_file_extension(path, ".yml"):
                continue

            try:
                with open(path, encoding="utf-8") as file:
                    proto = self.create_proto(file)
                    data = yaml.safe_load(proto)
            except Exception as e:
                with open(self.errors_path, "a") as file:
                    file.write(f"YML-ERROR:\nAn error occurred during prototype processing {path}, error - {e}\n")
            else:
                if data is not None:
                    for prototype in data:
                        if self.check_proto_attrs(prototype):
                            self.get_proto_attrs(prototypes, prototype)

        return prototypes
