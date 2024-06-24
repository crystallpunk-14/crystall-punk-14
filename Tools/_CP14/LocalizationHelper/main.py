import json
from yml_parser import yml_parser
from ftl_parser import ftl_parser
from fluent import ftl_writer

def read_config():
    with open("config.json", "r", encoding="utf-8") as file:
        return json.load(file)

def get_paths(config: dict):
    prototypes_path = config["paths"]["prototypes"]
    localization_path = config["paths"]["localization"]
    return prototypes_path, localization_path

def check_attrs(values, prototypes_dict, attr):
    if not values.get(attr):
        parent = prototypes_dict.get(values["parent"])
        if parent and parent.get(attr):
            values[attr] = f"{{ ent-{values['parent']}.{attr} }}"

def main():
    """
        The function reads the config, gets paths to prototypes and localization files,
        and through parsers gets two dictionaries with information about prototypes.
    """	
    config = read_config()
    prototypes_path, localization_path = get_paths(config)
    prototypes_dict, localization_dict = yml_parser.yml_parser(prototypes_path), ftl_parser.ftl_parser(localization_path)

    entities_ftl = ""

    """
        First, the function checks for the prototype in the dictionary with
        prototypes from localization files, if it is there,
        it uses data from localization files, if not, it checks already in the dictionary 
        with prototypes, and then there are different checks to find parents.
    """
    for prototype in prototypes_dict:
        if localization_dict.get(prototype):
            proto_ftl = ftl_writer.create_ftl(prototype, localization_dict[prototype])
            entities_ftl += proto_ftl
        elif prototypes_dict.get(prototype):
            values = prototypes_dict[prototype]
            if not values.get("name"):
                values["name"] = f"{{ ent-{values['parent']} }}"

                check_attrs(values, prototypes_dict, "desc")
                check_attrs(values, prototypes_dict, "suffix")

            proto_ftl = ftl_writer.create_ftl(prototype, prototypes_dict[prototype])
            entities_ftl += proto_ftl

    with open("entities.ftl", "w", encoding="utf-8") as file:
        file.write(entities_ftl)

if __name__ == '__main__':
    main()