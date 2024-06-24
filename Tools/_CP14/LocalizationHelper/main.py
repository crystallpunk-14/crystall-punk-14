from yml_parser import yml_parser
from ftl_parser import ftl_parser
from fluent import ftl_writer
import json


def read_config():
    with open("config.json", "r", encoding="utf-8") as file:
        return json.load(file)


def get_paths(config: dict):
    prototypes_path = config["paths"]["prototypes"]
    localization_path = config["paths"]["localization"]
    return prototypes_path, localization_path


def main():
    config = read_config()
    prototypes_path, localization_path = get_paths(config)
    prototypes_dict, localization_dict = yml_parser.yml_parser(prototypes_path), ftl_parser.ftl_parser(localization_path)

    entities_ftl = ""

    for prototype in prototypes_dict:
        if localization_dict.get(prototype):
            proto_ftl = ftl_writer.create_ftl(prototype, localization_dict[prototype])
            entities_ftl += proto_ftl
        elif prototypes_dict.get(prototype):
            values = prototypes_dict[prototype]
            if not values.get("name"):
                values["name"] = f"{{ ent-{values['parent']} }}"

                def check_attrs(attr):
                    if not values.get(attr):
                        parent = prototypes_dict.get(values["parent"])
                        if parent and parent.get(attr):
                            values[attr] = f"{{ ent-{values['parent']}.{attr} }}"

                check_attrs("desc")
                check_attrs("suffix")

            proto_ftl = ftl_writer.create_ftl(prototype, prototypes_dict[prototype])
            entities_ftl += proto_ftl

    with open("entities.ftl", "w", encoding="utf-8") as file:
        file.write(entities_ftl)


if __name__ == '__main__':
    main()
