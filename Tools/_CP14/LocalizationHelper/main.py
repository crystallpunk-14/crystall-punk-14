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


def main():
    """
        The function reads the config, gets paths to prototypes and localization files,
        and through parsers gets two dictionaries with information about prototypes,
        and creates one common vocabulary.
    """

    with open("logs/errors_log.txt", "w") as file:
        file.write("")

    config = read_config()
    prototypes_path, localization_path = get_paths(config)
    prototypes_dict, localization_dict = yml_parser.yml_parser(prototypes_path), ftl_parser.ftl_parser(localization_path)

    all_prototypes = {**prototypes_dict, **localization_dict}

    entities_ftl = ""

    """
        The function traverses each prototype from the dictionary, checks if it has a parent,
        and performs certain checks on the attributes of the parent if the prototype does not have its own attributes.
    """

    for prototype in all_prototypes:
        prototype_attrs = all_prototypes[prototype]

        try:
            if prototype in prototypes_dict:
                prototype_attrs["parent"] = prototypes_dict[prototype]["parent"]

                parent = prototype_attrs["parent"]
                if not prototype_attrs.get("name"):
                    prototype_attrs["name"] = f"{{ ent-{prototype_attrs["parent"]} }}"

                if not prototype_attrs["desc"]:
                    if parent and not isinstance(parent, list) and prototypes_dict.get(parent):
                        prototype_attrs["desc"] = f"{{ ent-{parent}.desc }}"

            proto_ftl = ftl_writer.create_ftl(prototype, all_prototypes[prototype])
            entities_ftl += proto_ftl

        except Exception as e:
            with open("logs/errors_log.txt", "a") as file:
                file.write(f"RETRIEVING-ERROR:\nAn error occurred while retrieving data to be written to the file - {e}\n")


    file_name = "entities.ftl"
    with open(file_name, "w", encoding="utf-8") as file:
        file.write(entities_ftl)

    print(f"{file_name} has been created\n")

    with open("logs/errors_log.txt", "r") as file:
        errors = file.read()

    successful_count = len(all_prototypes) - errors.count("ERROR")
    print(f"""Of the {len(all_prototypes)} prototypes, {successful_count} were successfully processed.
Errors can be found in 'logs/errors_log.txt'.
Number of errors during YML processing - {errors.count("YML-ERROR")}
Number of errors during FTL processing - {errors.count("FTL-ERROR")}
Number of errors during data extraction and creation of new FTL  - {errors.count("RETRIEVING-ERROR")}""")


if __name__ == '__main__':
    main()