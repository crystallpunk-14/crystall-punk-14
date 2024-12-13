from PIL import Image
import os

from color_replacer.rgb_container import RGBPallet


class ColorReplacer:
    def __init__(self, replace_from: str, replace_to: str,
                 replace_from_pallet: RGBPallet, replace_to_pallet: RGBPallet):
        self.replace_from = replace_from
        self.replace_to = replace_to
        self.replace_from_pallet = replace_from_pallet
        self.replace_to_pallet = replace_to_pallet
        self.pallets = self._create_color_map()

    def _create_color_map(self) -> dict[tuple, tuple]:
        color_map = {}
        replace_to_pallet_len = len(self.replace_to_pallet.pallet)
        for i in range(len(self.replace_from_pallet.pallet)):
            if i == replace_to_pallet_len:
                break

            replace_from_pallet_tuple = tuple(self.replace_from_pallet.pallet[i].rgba_list)
            color_map[replace_from_pallet_tuple] = tuple(self.replace_to_pallet.pallet[i].rgba_list)
        return color_map

    def _init_dirs(self):
        if not os.path.exists(self.replace_to):
            os.mkdir(self.replace_to)
        return os.listdir(self.replace_from)

    def replace_colors(self):
        sprites = self._init_dirs()

        for sprite in sprites:
            if not sprite.endswith("png"):
                continue

            path = f"{self.replace_from}/{sprite}"
            image = Image.open(path).convert("RGBA")
            pixels = list(image.getdata())
            new_pixels = [(self.pallets.get(pixel, pixel)) for pixel in pixels]
            image.putdata(new_pixels)
            image.save(f"{self.replace_to}/{sprite}")
