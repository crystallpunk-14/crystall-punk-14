class Prototype:
    def __init__(self, prototype: dict):
        self._initialize_attrs(prototype)

    def _initialize_attrs(self, prototype: dict):
        self.name = prototype.get("name")
        self.description = prototype.get("description")
        self.parent = prototype.get("parent")
        self.id = prototype.get("id")
        self.suffix = prototype.get("suffix")
        self.attrs_dict = {
            "id": self.id,
            "name": self.name,
            "description": self.description,
            "parent": self.parent,
            "suffix": self.suffix
        }

    def reinitialize_with_attrs_dict(self):
        self._initialize_attrs(self.attrs_dict)

    def __repr__(self):
        return str(self.attrs_dict)


def check_prototype_attrs(prototype: Prototype, with_parent_check: bool = True) -> bool:

    if prototype.name:
        return True
    elif prototype.description:
        return True
    elif prototype.suffix:
        return True
    # In some cases a parent can be a list (because of multiple parents),
    # the game will not be able to handle such cases in ftl files.
    elif with_parent_check and prototype.parent and not isinstance(prototype.parent, list):
        return True

    return False
