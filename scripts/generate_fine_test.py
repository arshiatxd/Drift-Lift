import cv2
import numpy as np
from PIL import Image
import os

xbox_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox"
base_orig = Image.open(os.path.join(xbox_dir, "XB_Base.png")).convert("RGBA")

canvas_w, canvas_h = 420, 260
scale = 260.0 / 954.0
scaled_w = int(round(1534.0 * scale))
scaled_h = 260
dx = (canvas_w - scaled_w) / 2.0
dy = 0.0

base_scaled = base_orig.resize((scaled_w, scaled_h), Image.Resampling.LANCZOS)

# Let's test fine-tuning:
# Centers on 420x260 Canvas:
# D-pad center on canvas:
# In 1534x954, D-pad center is (577.8, 655.4).
# Canvas D-pad center = dx + 577.8 * scale = 0.96 + 157.48 = 158.4, y = 655.4 * scale = 178.6.
#
# D-pad Up arm center: (158.4, 163.0)
# D-pad Down arm center: (158.4, 194.5)
# D-pad Left arm center: (144.0, 178.6)
# D-pad Right arm center: (172.8, 178.6)
#
# ABXY centers on Canvas:
# Y center: (323.1, 84.5)
# X center: (293.0, 114.5)
# B center: (352.7, 111.2)
# A center: (321.9, 141.0)
#
# ABXY sizes:
# If scaled to 36x35 (scale factor ~0.82 of halo), the cyan glow circle fits snugly over each button cap!

test_configs = [
    # ABXY Buttons:
    ("XBSeries_Y_Button.png", 323.1 - 38/2, 84.5 - 36/2, 38.0, 36.0),
    ("XBSeries_X_Button.png", 293.0 - 37/2, 114.5 - 35/2, 37.0, 35.0),
    ("XBSeries_B_Button.png", 352.7 - 37/2, 111.2 - 35/2, 37.0, 35.0),
    ("XBSeries_A_Button.png", 321.9 - 36/2, 141.0 - 35/2, 36.0, 35.0),

    # D-Pad:
    ("XBSeries_D-PAD_Up.png", 158.4 - 24/2, 163.0 - 22/2, 24.0, 22.0),
    ("XBSeries_D-PAD_Down.png", 158.4 - 23/2, 194.5 - 23/2, 23.0, 23.0),
    ("XBSeries_D-PAD_Left.png", 144.0 - 27/2, 178.6 - 22/2, 27.0, 22.0),
    ("XBSeries_D-PAD_Right.png", 172.8 - 26/2, 178.6 - 21/2, 26.0, 21.0),

    # Bumpers:
    ("XBSeries_LeftBumper_Active.png", 39.2, 42.2, 135.7, 46.9),
    ("XBSeries_RightBumper_Active.png", 245.2, 42.2, 136.5, 46.6),

    # Triggers:
    ("XBSeries_LeftTrigger_Active.png", 66.4, 0.0, 66.8, 51.8),
    ("XBSeries_RightTrigger_Active.png", 287.2, 0.0, 65.7, 45.8),

    # System Buttons:
    ("XBSeries_ViewButton.png", 162.9, 101.4, 31.3, 28.9),
    ("XBSeries_MenuButton.png", 225.8, 101.4, 31.3, 28.9),
    ("XBSeries_ShareButton.png", 191.0, 128.4, 37.1, 21.0),
    ("XBSeries_HomeButton.png", 184.4, 59.7, 51.0, 48.2),

    # Sticks:
    ("XBSeries_LeftStick.png", 76.8, 89.9, 48.2, 48.0),
    ("XBSeries_RightStick.png", 241.9, 156.2, 49.1, 46.3),
]

canvas_fine = Image.new("RGBA", (canvas_w, canvas_h), (0, 0, 0, 255))
canvas_fine.paste(base_scaled, (int(round(dx)), int(round(dy))), base_scaled)

for fname, cx, cy, cw, ch in test_configs:
    p = os.path.join(xbox_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas_fine.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))

out_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_canvas_fine.png"
canvas_fine.save(out_path)
print(f"Saved {out_path}")
