#!/usr/bin/env python3
"""Generate pixel art for Moat: Charms of Chiang Mai.

Outputs (into MoatCharms/assets/):
  charms.png          128x16 — 8 cells of 16x16: 6 amulets + omen bird + resting pouch
  seller.png          64x128 — Lung Saeng character sheet (16 frames of 16x32, static vendor)
  seller_portrait.png 64x64  — Lung Saeng portrait
"""
from pathlib import Path
from PIL import Image, ImageDraw

ASSETS = Path(__file__).resolve().parent.parent / "MoatCharms" / "assets"

# palette
K = (45, 33, 30, 255)      # outline
CREAM = (245, 238, 220, 255)
CREAM_D = (219, 208, 182, 255)
GOLD = (232, 182, 62, 255)
GOLD_D = (178, 128, 32, 255)
RED = (198, 58, 48, 255)
RED_D = (148, 40, 34, 255)
WOOD = (152, 102, 58, 255)
WOOD_D = (110, 70, 40, 255)
ORANGE = (231, 141, 48, 255)
ORANGE_D = (186, 98, 28, 255)
JADE = (168, 214, 168, 255)
JADE_D = (106, 168, 118, 255)
SKIN = (224, 178, 138, 255)
SKIN_D = (188, 138, 98, 255)
DARK = (58, 48, 44, 255)
CHESTNUT = (170, 92, 48, 255)
INDIGO = (56, 62, 92, 255)


def cell(draw, cx):
    """Return a draw helper offset to cell cx (16px cells)."""
    ox = cx * 16

    def rect(x0, y0, x1, y1, fill, outline=None):
        draw.rectangle([ox + x0, y0, ox + x1, y1], fill=fill, outline=outline)

    def ell(x0, y0, x1, y1, fill, outline=None):
        draw.ellipse([ox + x0, y0, ox + x1, y1], fill=fill, outline=outline)

    def px(x, y, fill):
        draw.point((ox + x, y), fill=fill)

    def line(x0, y0, x1, y1, fill):
        draw.line([ox + x0, y0, ox + x1, y1], fill=fill)

    return rect, ell, px, line


def make_charms():
    img = Image.new("RGBA", (128, 16), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    # 0 — phra somdej: cream tablet, gold arch, seated figure
    rect, ell, px, line = cell(d, 0)
    line(7, 0, 8, 0, RED)
    line(6, 1, 9, 1, RED_D)
    rect(4, 2, 11, 14, CREAM, K)
    rect(5, 13, 10, 14, CREAM_D)
    ell(5, 3, 10, 11, GOLD, GOLD_D)
    rect(7, 5, 8, 7, WOOD_D)
    rect(6, 8, 9, 9, WOOD_D)

    # 1 — takrut: rolled gold scroll on a red cord
    rect, ell, px, line = cell(d, 1)
    line(0, 4, 15, 4, RED)
    line(0, 5, 15, 5, RED_D)
    rect(2, 6, 13, 10, GOLD, GOLD_D)
    rect(2, 6, 3, 10, GOLD_D)
    rect(12, 6, 13, 10, GOLD_D)
    for x in (6, 9):
        line(x, 6, x, 10, GOLD_D)
    line(3, 7, 12, 7, CREAM)

    # 2 — sing: proud carved lion
    rect, ell, px, line = cell(d, 2)
    ell(6, 1, 14, 9, ORANGE_D)          # mane
    ell(7, 2, 13, 8, ORANGE, K)         # head
    px(9, 4, DARK); px(11, 4, DARK)     # eyes
    line(9, 6, 11, 6, ORANGE_D)         # muzzle
    rect(3, 8, 11, 13, ORANGE, K)       # body
    rect(3, 12, 4, 14, ORANGE_D)        # foreleg
    rect(9, 12, 10, 14, ORANGE_D)
    line(2, 8, 1, 5, ORANGE_D)          # tail

    # 3 — palad khik: discreet carved wood, best kept pocketed
    rect, ell, px, line = cell(d, 3)
    ell(5, 1, 9, 6, WOOD, WOOD_D)
    rect(5, 4, 9, 11, WOOD, None)
    line(5, 4, 5, 11, WOOD_D)
    line(9, 4, 9, 11, WOOD_D)
    rect(3, 11, 11, 13, WOOD_D, K)
    line(7, 5, 7, 10, WOOD_D)           # grain

    # 4 — nang kwak: the beckoning lady, hand raised
    rect, ell, px, line = cell(d, 4)
    ell(5, 2, 9, 6, SKIN, SKIN_D)       # face
    rect(6, 1, 8, 2, DARK)              # hair
    line(10, 5, 11, 2, SKIN)            # raised beckoning arm
    px(11, 1, SKIN)
    rect(4, 6, 10, 12, RED, RED_D)      # dress
    rect(3, 12, 11, 13, GOLD, GOLD_D)   # kneeling base / cushion
    px(6, 8, GOLD); px(8, 8, GOLD)      # sash sparkle

    # 5 — jing-jok: pale jade house gecko
    rect, ell, px, line = cell(d, 5)
    ell(2, 3, 6, 7, JADE, JADE_D)       # head
    ell(4, 5, 11, 10, JADE, JADE_D)     # body
    line(11, 8, 13, 11, JADE_D)         # tail
    px(14, 12, JADE_D)
    px(4, 5, DARK)                      # eye
    for lx, ly in ((4, 10), (9, 10), (5, 4), (10, 6)):
        px(lx, ly, JADE_D)              # toes

    # 6 — omen coucal: dark bird, chestnut wing, red eye
    rect, ell, px, line = cell(d, 6)
    ell(3, 5, 11, 11, DARK)             # body
    ell(8, 2, 13, 7, DARK)              # head
    rect(5, 7, 9, 9, CHESTNUT)          # wing
    line(3, 10, 1, 14, DARK)            # long tail
    line(4, 11, 2, 14, DARK)
    px(11, 4, RED)                      # the coucal's red eye
    line(13, 5, 14, 5, GOLD_D)          # beak

    # 7 — resting pouch: charms asleep in the pocket
    rect, ell, px, line = cell(d, 7)
    ell(3, 6, 12, 14, WOOD, WOOD_D)     # bag
    rect(6, 4, 9, 7, WOOD_D)            # neck
    line(5, 5, 10, 5, RED)              # drawstring
    px(4, 8, CREAM)                     # highlight
    px(5, 9, CREAM)

    ASSETS.mkdir(parents=True, exist_ok=True)
    img.save(ASSETS / "charms.png")


def draw_saeng_frame(d, ox, oy):
    """One 16x32 frame of Lung Saeng, facing front: straw hat, cream shirt, amulet cords."""
    def rect(x0, y0, x1, y1, fill, outline=None):
        d.rectangle([ox + x0, oy + y0, ox + x1, oy + y1], fill=fill, outline=outline)

    def ell(x0, y0, x1, y1, fill, outline=None):
        d.ellipse([ox + x0, oy + y0, ox + x1, oy + y1], fill=fill, outline=outline)

    def px(x, y, fill):
        d.point((ox + x, oy + y), fill=fill)

    def line(x0, y0, x1, y1, fill):
        d.line([ox + x0, oy + y0, ox + x1, oy + y1], fill=fill)

    # straw hat
    ell(2, 5, 13, 8, GOLD, GOLD_D)
    ell(5, 2, 10, 7, GOLD, GOLD_D)
    # face
    rect(5, 8, 10, 12, SKIN, None)
    px(6, 9, DARK); px(9, 9, DARK)             # eyes
    line(6, 11, 9, 11, DARK)                   # mustache
    # shirt
    rect(4, 13, 11, 21, CREAM, CREAM_D)
    rect(3, 14, 4, 18, CREAM_D)                # sleeves
    rect(11, 14, 12, 18, CREAM_D)
    # amulet cords
    line(6, 13, 7, 16, RED)
    line(9, 13, 8, 16, RED)
    px(7, 17, GOLD_D); px(8, 17, GOLD_D)       # amulet at chest
    # sarong
    rect(4, 22, 11, 26, INDIGO)
    line(4, 22, 11, 22, RED_D)                 # waist tie
    # legs + sandals
    rect(5, 27, 6, 30, SKIN_D)
    rect(9, 27, 10, 30, SKIN_D)
    rect(4, 30, 7, 31, WOOD_D)
    rect(8, 30, 11, 31, WOOD_D)


def make_seller():
    img = Image.new("RGBA", (64, 128), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    for row in range(4):
        for col in range(4):
            draw_saeng_frame(d, col * 16, row * 32)
    img.save(ASSETS / "seller.png")


def make_portrait():
    img = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    # straw hat
    d.ellipse([6, 18, 57, 30], fill=GOLD, outline=GOLD_D)
    d.ellipse([18, 6, 45, 26], fill=GOLD, outline=GOLD_D)
    d.line([10, 24, 53, 24], fill=GOLD_D)
    # face
    d.rectangle([20, 28, 43, 48], fill=SKIN)
    d.rectangle([18, 32, 20, 42], fill=SKIN_D)  # ears
    d.rectangle([43, 32, 45, 42], fill=SKIN_D)
    # smiling eyes (closed-with-joy arcs)
    d.arc([23, 33, 29, 39], start=180, end=360, fill=DARK)
    d.arc([34, 33, 40, 39], start=180, end=360, fill=DARK)
    # nose + mustache + smile
    d.line([31, 38, 31, 41], fill=SKIN_D)
    d.line([25, 44, 38, 44], fill=DARK)
    d.arc([26, 42, 37, 50], start=20, end=160, fill=RED_D)
    # sun-creased cheeks
    d.point((22, 42), fill=SKIN_D)
    d.point((41, 42), fill=SKIN_D)
    # shoulders + shirt
    d.rectangle([12, 49, 51, 63], fill=CREAM, outline=CREAM_D)
    # amulet cords with two charms
    d.line([24, 49, 27, 58], fill=RED)
    d.line([39, 49, 36, 58], fill=RED)
    d.rectangle([26, 58, 28, 61], fill=GOLD, outline=GOLD_D)
    d.rectangle([35, 58, 37, 61], fill=WOOD, outline=WOOD_D)
    img.save(ASSETS / "seller_portrait.png")


if __name__ == "__main__":
    make_charms()
    make_seller()
    make_portrait()
    print(f"sprites written to {ASSETS}")
