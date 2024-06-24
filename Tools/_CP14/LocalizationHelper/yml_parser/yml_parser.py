import yaml
import os


def check_proto_attrs(prototype: dict) -> bool:
    return any(prototype.get(attr) is not None for attr in ["name", "description", "suffix"])


def get_proto_attrs(prototypes: dict, prototype: dict) -> None:
    prototypes[prototype.get("id")] = {
        "parent": prototype.get("parent"),
        "name": prototype.get("name"),
        "desc": prototype.get("description"),
        "suffix": prototype.get("suffix")
    }


def yml_parser(path: str) -> dict:
    """
        The function gets the path, then with the help of the os library
        goes through each file,checks that the file extension is "yml",
        then processes the file using the "PyYaml" library
    """
    prototypes = {}

    for dirpath, _, filenames in os.walk(path):
        for filename in filenames:
            path = f"{dirpath}/{filename}"

            if not filename.endswith(".yml"):
                continue

            try:
                with open(path, encoding="utf-8") as file:
                    proto = ""
                    for line in file.readlines():
                        # The PyYaml library cannot handle the following SpaceStation 14 prototype syntax - !type: ...
                        if "!type" in line:
                            continue
                        proto += line

                    data = yaml.safe_load(proto)
            except Exception as e:
                print(f"An error occurred during prototype processing - {e}")
                print(path)
            else:
                if data is not None:
                    for prototype in data:
                        if check_proto_attrs(prototype):
                            get_proto_attrs(prototypes, prototype)

    return prototypes
