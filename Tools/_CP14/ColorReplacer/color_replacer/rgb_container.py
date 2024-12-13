class InvalidRGBColor(Exception):
    pass


class InvalidHexLen(Exception):
    pass


class RGBContainer:
    def __init__(self, red: int = 0, green: int = 0, blue: int = 0, alpha: int = 255):
        self._red = self._check_channel_number(red)
        self._green = self._check_channel_number(green)
        self._blue = self._check_channel_number(blue)
        self._alpha = self._check_channel_number(alpha)
        self.rgba_list = [red, green, blue, alpha]

    @classmethod
    def from_hex(cls, hex_color: str):
        hex_color = hex_color.lstrip('#')

        if len(hex_color) not in [6, 8]:
            raise InvalidHexLen("HEX string length must be 6 or 8 characters.")

        if len(hex_color) == 6:
            hex_color += 'FF'

        red = int(hex_color[0:2], 16)
        green = int(hex_color[2:4], 16)
        blue = int(hex_color[4:6], 16)
        alpha = int(hex_color[6:8], 16)

        return RGBContainer(red, green, blue, alpha)

    @staticmethod
    def _check_channel_number(value: int):
        if not 0 <= value <= 255:
            raise InvalidRGBColor("Channel number must be in the range from 0 to 255")
        return value

    def __repr__(self):
        return f"({self._red}, {self._green}, {self._blue}, {self._alpha})"

    @property
    def red(self):
        return self._red

    @red.setter
    def red(self, new_red: int):
        self._red = self._check_channel_number(new_red)
        self.rgba_list[0] = new_red

    @property
    def green(self):
        return self._green

    @green.setter
    def green(self, new_green: int):
        self._green = self._check_channel_number(new_green)
        self.rgba_list[1] = new_green

    @property
    def blue(self):
        return self._blue

    @blue.setter
    def blue(self, new_blue: int):
        self._blue = self._check_channel_number(new_blue)
        self.rgba_list[2] = new_blue

    @property
    def alpha(self):
        return self._alpha

    @alpha.setter
    def alpha(self, new_alpha: int):
        self._alpha = self._check_channel_number(new_alpha)
        self.rgba_list[3] = new_alpha


class RGBPallet:
    def __init__(self, pallet: list[RGBContainer] | None = None):
        self._pallet = pallet if pallet else []

    @property
    def pallet(self):
        return self._pallet

    def create_new_rgb_container(self, red: int = 0, green: int = 0, blue: int = 0, alpha: int = 255):
        self._pallet.append(RGBContainer(red, green, blue, alpha))

    def create_new_rgb_container_from_hex(self, hex_color: str):
        self._pallet.append(RGBContainer.from_hex(hex_color))
