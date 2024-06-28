import json
import os
from yml_parser import YMLParser
from ftl_parser import FTLParser
from fluent import ftl_writer


def read_config():
    with open("config.json", "r", encoding="utf-8") as file:
        return json.load(file)


def get_paths(config: dict):
    prototypes_path = config["paths"]["prototypes"]
    localization_path = config["paths"]["localization"]
    errors_log_path = config["paths"]["error_log_path"]
    yml_parser_last_launch = config["paths"]["yml_parser_last_launch"]
    return prototypes_path, localization_path, errors_log_path, yml_parser_last_launch


def print_errors_log_info(errors_log_path: str, all_prototypes: dict) -> None:
    with open(errors_log_path, "r") as file:
        errors = file.read()

    successful_count = len(all_prototypes) - errors.count("ERROR")
    print(f"""Of the {len(all_prototypes)} prototypes, {successful_count} were successfully processed.

Errors can be found in  {errors_log_path}
Number of errors during YML processing - {errors.count("YML-ERROR")}
Number of errors during FTL processing - {errors.count("FTL-ERROR")}
Number of errors during data extraction and creation of new FTL  - {errors.count("RETRIEVING-ERROR")}""")


def check_changed_attrs(yml_parser_last_launch: str, prototypes_dict: dict, localization_dict: dict):
    """

    What error it fixes - without this function, changed attributes of prototypes that have not been changed in
    localization files will simply not be added to the original ftl file, because the script first of all takes data
    from localization files, if they exist, of course

    The function gets the data received during the last run of the script, and checks if some attribute from
    the last run has been changed,then simply replaces with this attribute the attribute
    of the prototype received during parsing of localization files.
    """
    if os.path.isfile(yml_parser_last_launch):
        with open(yml_parser_last_launch, 'r', encoding='utf-8') as file:
            last_launch_prototypes = json.load(file)

        for prototype, proto_attrs_in_prototypes in prototypes_dict.items():
            if prototype in last_launch_prototypes and prototype in localization_dict:
                attrs = localization_dict[prototype]
                last_launch_prototype_attrs = last_launch_prototypes[prototype]

                for key, value in proto_attrs_in_prototypes.items():
                    if value != last_launch_prototype_attrs[key]:
                        attrs[key] = value

                localization_dict[prototype] = attrs


def save_result(entities: str, file_name: str) -> None:
    with open(file_name, "w", encoding="utf-8") as file:
        file.write(entities)

    print(f"{file_name} has been created\n")

def main():
    """
    The function gets paths, creates dictionaries with the help of parsers,
    performs various checks, and finally creates ftl file.
    """

    config = read_config()
    prototypes_path, localization_path, errors_log_path, yml_parser_last_launch = get_paths(config)

    if not os.path.isdir("last_launch"):
        os.mkdir("last_launch")
        
    with open(errors_log_path, "w") as file:
        file.write("")

    yml_parser = YMLParser((prototypes_path, errors_log_path))
    prototypes_dict = yml_parser.yml_parser()

    ftl_parser = FTLParser((localization_path, errors_log_path))
    localization_dict = ftl_parser.ftl_parser()

    check_changed_attrs(yml_parser_last_launch, prototypes_dict, localization_dict)

    with open(yml_parser_last_launch, 'w') as json_file:
        json.dump(prototypes_dict, json_file, indent=4)

    # This is where the two dictionaries are merged, and prototypes from
    # the localization_dict dictionary are preferably selected.
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

                if not isinstance(parent, list) and parent in prototypes_dict:
                    if not prototype_attrs.get("name"):
                        prototype_attrs["name"] = f"{{ ent-{parent} }}"

                    if not prototype_attrs.get("desc"):
                        if parent and not isinstance(parent, list) and prototypes_dict.get(parent):
                            prototype_attrs["desc"] = f"{{ ent-{parent}.desc }}"

                if not prototype_attrs.get("suffix"):
                    if prototypes_dict[prototype].get("suffix"):
                        prototype_attrs["suffix"] = prototypes_dict[prototype]["suffix"]

            if any(prototype_attrs[attr] is not None for attr in ["name", "desc", "suffix"]):
                proto_ftl = ftl_writer.create_ftl(prototype, all_prototypes[prototype])
                entities_ftl += proto_ftl

        except Exception as e:
            with open(errors_log_path, "a") as file:
                print(prototype, prototype_attrs)
                file.write(f"RETRIEVING-ERROR:\nAn error occurred while retrieving data to be written to the file - {e}\n")

    file_name = "entities.ftl"
    save_result(entities_ftl, file_name)

    print_errors_log_info(errors_log_path, all_prototypes)


if __name__ == '__main__':
    main()