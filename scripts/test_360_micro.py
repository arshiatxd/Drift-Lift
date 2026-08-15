import cv2
import numpy as np
from PIL import Image
import os

base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360\XB360_Base.png"
press_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360"

base = Image.open(base_path).convert("RGBA")
scale = 260.0 / 955.0
dx = (420.0 - 1545.0 * scale) / 2.0
dy = 0.0

items_micro = [
    # BUMPERS & TRIGGERS
    ("XB360_LeftBumper_Active.png", 36.4, 34.8, 84.9, 38.4),
    ("XB360_RightBumper_Active.png", 307.1, 34.0, 77.6, 38.4),
    ("XB360_LeftTrigger_Active.png", 75.1, 2.2, 38.9, 41.4),
    ("XB360_RightTrigger_Active.png", 313.3, 1.6, 38.9, 41.4),

    # D-PAD
    ("XB360_D-PAD_Up.png", 132.3, 165.5, 29.4, 31.0),
    ("XB360_D-PAD_Down.png", 132.3, 196.3, 29.4, 31.0),
    ("XB360_D-PAD_Left.png", 110.5, 183.5, 36.5, 29.4),
    ("XB360_D-PAD_Right.png", 146.2, 181.9, 36.5, 29.1),

    # ABXY Buttons
    ("XB360_Y_Button.png", 323.5, 85.5, 35.1, 32.1),
    ("XB360_B_Button.png", 358.2, 113.4, 33.2, 31.3),
    ("XB360_X_Button.png", 290.2, 113.7, 34.3, 30.8),
    ("XB360_A_Button.png", 323.8, 140.0, 34.6, 28.9),

    # Guide, Back, Start
    ("XB360_GuideButton.png", 187.1, 110.1, 46.6, 37.8),
    ("XB360_BackButton.png", 149.2, 120.2, 25.0, 17.7),
    ("XB360_StartButton.png", 246.6, 120.2, 25.0, 17.7),

    # Sticks
    ("XB360_LeftStick_Black.png", 57.7, 101.5, 49.0, 49.0),
    ("XB360_RightStick_Black.png", 233.0, 163.5, 49.0, 49.0),
]

scaled_w = int(round(1545.0 * scale))
base_scaled = base.resize((scaled_w, 260), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (420, 260), (0, 0, 0, 255))
canvas.paste(base_scaled, (int(round(dx)), int(round(dy))), base_scaled)

for fname, cx, cy, cw, ch in items_micro:
    p = os.path.join(press_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))

canvas.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_360_micro.png")
