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

    @staticmethod
    def _fix_type_error_with_ignore(proto: str) -> str:
        """
        Removes lines containing '!type' from the prototype string.

        Args:
            proto (str): The prototype string to be processed.

        Returns:
            str: The prototype string with lines containing '!type' removed.
        """
        proto_lines = proto.splitlines()
        fixed_proto = "\n".join(line for line in proto_lines if "!type" not in line)
        return fixed_proto

    def _load_proto(self, file, path) -> list[dict]:
        """
        Loads and processes YAML prototypes from the file.

        Args:
            file: The file object containing YAML data.
            path (str): The path of the file used for error reporting.

        Returns:
            list[dict]: A list of dictionaries representing the prototypes.

        Note:
            Error handling is designed so that if one prototype fails to process, other prototypes
            in the same file will still be processed. Here's how it works:

            1. **Prototype Splitting:** The file is read and split into prototypes using a regular expression.
               Each prototype is processed separately.

            2. **Error Handling for Each Prototype:**
               - **First Attempt:** If `yaml.safe_load` cannot process the prototype, it tries replacing `!type:`
                 strings and loads YAML again.
               - **Second Attempt:** If that fails, it applies `_fix_type_error_with_ignore` to remove lines
                 with `!type` and attempts to load YAML again.
               - **Exception Handling:** If errors occur in both attempts, they are logged to an error file,
                 and the current prototype is skipped.

            3. **Adding Successful Data:** If the prototype is successfully processed, it is added to the
               `prototypes` list.
        """
        content_str = file.read()
        prototypes_lst = re.split(r"\n(?=- type:)", content_str)

        prototypes = []
        for proto in prototypes_lst:
            try:
                fixed_proto = proto.replace("!type:", "type: bobo")
                data = yaml.safe_load(fixed_proto)
                if data is None:
                    continue
                data = data[0]
            except Exception:
                try:
                    fixed_proto = self._fix_type_error_with_ignore(proto)
                    data = yaml.safe_load(fixed_proto)[0]
                except Exception as e:
                    with open(self.errors_path, "a") as error_file:
                        error_file.write(
                            f"YML-ERROR:\nAn error occurred during prototype processing {path}, error - {e}\n")
                    continue

            prototypes.append(data)
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
