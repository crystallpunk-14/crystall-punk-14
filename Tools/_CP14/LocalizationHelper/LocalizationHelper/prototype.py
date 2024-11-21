class Prototype:
    def __init__(self, prototype: dict):
        self._name = prototype.get("name")
        self._description = prototype.get("description")
        self._parent = prototype.get("parent")
        self._id = prototype.get("id")
        self._suffix = prototype.get("suffix")
        self._attrs_dict = {
            "id": self._id,
            "name": self._name,
            "description": self._description,
            "parent": self._parent,
            "suffix": self._suffix
        }

    @property
    def name(self):
        return self._name

    @name.setter
    def name(self, new_name: str):
        self._name = new_name
        self._attrs_dict["name"] = new_name

    @property
    def description(self):
        return self._description

    @description.setter
    def description(self, new_description: str):
        self._description = new_description
        self._attrs_dict["description"] = new_description

    @property
    def parent(self):
        return self._parent

    @parent.setter
    def parent(self, new_parent: str):
        self._parent = new_parent
        self._attrs_dict["parent"] = new_parent

    @property
    def id(self):
        return self._id

    @id.setter
    def id(self, new_id: str):
        self._id = new_id
        self._attrs_dict["id"] = new_id

    @property
    def suffix(self):
        return self._suffix

    @suffix.setter
    def suffix(self, new_suffix: str):
        self._suffix = new_suffix
        self._attrs_dict["suffix"] = new_suffix

    @property
    def attrs_dict(self):
        return self._attrs_dict

    def set_attrs_dict_value(self, key, value):
        self._attrs_dict[key] = value
        if key == "name":
            self._name = value
        elif key == "description":
            self._description = value
        elif key == "id":
            self._id = value
        elif key == "parent":
            self._parent = value
        elif key == "suffix":
            self._suffix = value

    def __repr__(self):
        return str(self._attrs_dict)


def check_prototype_attrs(prototype: Prototype, with_parent_check: bool = True) -> bool:

    if prototype.name:
        # if prototype.id == "CP14BaseWooden":
        #     print(prototype.name)
        return True
    elif prototype.description:
        # if prototype.id == "CP14BaseWooden":
        #     print(prototype.description)
        return True
    elif prototype.suffix:
        return True
    # In some cases a parent can be a list (because of multiple parents),
    # the game will not be able to handle such cases in ftl files.
    elif with_parent_check and prototype.parent and not isinstance(prototype.parent, list):
        return True

    return False
