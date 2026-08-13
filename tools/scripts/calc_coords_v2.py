"""
Exact button mapping script for 840x520 PNG assets scaled to 420x260 Canvas.
Scale factor: 0.5x in both X and Y.
"""
from PIL import Image
import os

ASSETS_DIR = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets"

# Open images to verify dimensions
ps4_img = Image.open(os.path.join(ASSETS_DIR, "ps4_placeholder.png"))
xbox_img = Image.open(os.path.join(ASSETS_DIR, "xbox_placeholder.png"))

print(f"PS4 image size: {ps4_img.size}")
print(f"Xbox image size: {xbox_img.size}")

# 840x520 -> 420x260 means divide coordinates by 2!

def p(x840, y520, name):
    cx = round(x840 / 2)
    cy = round(y520 / 2)
    print(f"  {name:20s}: Center=({cx:3d}, {cy:3d}) | TopLeft=({cx-14:3d}, {cy-14:3d})")

print("\n--- PS4 420x260 Canvas Coordinates ---")
p(300, 260, "Left Stick")
p(540, 260, "Right Stick")
p(190, 160, "D-Pad Up")
p(190, 215, "D-Pad Down")
p(162, 187, "D-Pad Left")
p(218, 187, "D-Pad Right")
p(645, 120, "Triangle (Top)")
p(645, 250, "Cross (Bottom)")
p(580, 185, "Square (Left)")
p(710, 185, "Circle (Right)")
p(260, 90,  "Share")
p(580, 90,  "Options")
p(420, 115, "Touchpad")
p(240, 45,  "L1 Bumper")
p(240, 15,  "L2 Trigger")
p(600, 45,  "R1 Bumper")
p(600, 15,  "R2 Trigger")

print("\n--- Xbox 420x260 Canvas Coordinates ---")
p(230, 155, "Left Stick")
p(510, 265, "Right Stick")
p(325, 270, "D-Pad Center")
p(325, 230, "D-Pad Up")
p(325, 310, "D-Pad Down")
p(285, 270, "D-Pad Left")
p(365, 270, "D-Pad Right")
p(605, 100, "Y (Top)")
p(605, 205, "A (Bottom)")
p(550, 152, "X (Left)")
p(660, 152, "B (Right)")
p(360, 152, "View")
p(480, 152, "Menu")
p(420, 90,  "Xbox Guide")
p(230, 35,  "LB Bumper")
p(230, 10,  "LT Trigger")
p(610, 35,  "RB Bumper")
p(610, 10,  "RT Trigger")
