from color_replacer import RGBPallet

# Цвета в палитрах указаны от тёмных к более светлым.
# Colours in the palettes are listed from darker to lighter.

# Краткая Инструкция:
# Создать новую палитру можно через RGBPallet()
# Добавить цвета можно через rgba или hex
# Через rgba - обьект_палитры.create_new_rgb_container(красный, зелёный, синий, альфа канал)
# Через hex = обьект_палитры.create_new_rgb_container_from_hex(hex)

# Quick Instructions:
# Create a new palette using RGBPallet()
# Add colours via rgba or hex
# With rgba - palette object.create_new_rgb_container(red, green, blue, alpha channel)
# With hex = palette_object.create_new_rgb_container_from_hex(hex)

modular_copper_pallet = RGBPallet()
modular_copper_pallet.create_new_rgb_container_from_hex("#2f2825")
modular_copper_pallet.create_new_rgb_container_from_hex("#55433d")
modular_copper_pallet.create_new_rgb_container_from_hex("#754d38")
modular_copper_pallet.create_new_rgb_container_from_hex("#9a5e22")
modular_copper_pallet.create_new_rgb_container_from_hex("#b36510")
modular_copper_pallet.create_new_rgb_container_from_hex("#c57d07")

modular_gold_pallet = RGBPallet()
modular_gold_pallet.create_new_rgb_container_from_hex("#572116")
modular_gold_pallet.create_new_rgb_container_from_hex("#74371f")
modular_gold_pallet.create_new_rgb_container_from_hex("#b05b2c")
modular_gold_pallet.create_new_rgb_container_from_hex("#e88a36")
modular_gold_pallet.create_new_rgb_container_from_hex("#f1a94b")
modular_gold_pallet.create_new_rgb_container_from_hex("#ffe269")

modular_iron_pallet = RGBPallet()
modular_iron_pallet.create_new_rgb_container_from_hex("#282935")
modular_iron_pallet.create_new_rgb_container_from_hex("#3a3b4d")
modular_iron_pallet.create_new_rgb_container_from_hex("#5d5c71")
modular_iron_pallet.create_new_rgb_container_from_hex("#88899a")
modular_iron_pallet.create_new_rgb_container_from_hex("#b6b9cb")
modular_iron_pallet.create_new_rgb_container_from_hex("#e1e3e7")

mithril_pallet = RGBPallet()
mithril_pallet.create_new_rgb_container_from_hex("#052441")
mithril_pallet.create_new_rgb_container_from_hex("#024b76")
mithril_pallet.create_new_rgb_container_from_hex("#006c83")
mithril_pallet.create_new_rgb_container_from_hex("#1d8d7e")
mithril_pallet.create_new_rgb_container_from_hex("#52b695")
mithril_pallet.create_new_rgb_container_from_hex("#45d2a4")
