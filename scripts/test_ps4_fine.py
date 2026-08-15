from PIL import Image
import os

ps4_base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4\PS4_Base.png"
ps4_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4"

base = Image.open(ps4_base_path).convert("RGBA")
scale = 420.0 / 1466.0
dy = (260.0 - 783.0 * scale) / 2.0
dx = 0.0

ps4_elements_fine = [
    # Face Buttons (Triangle, Circle, Cross, Square)
    ("DS4_Face_Button.png", 322.1, 94.3, 28.4, 25.8, "Triangle"),
    ("DS4_Face_Button.png", 350.5, 119.8, 28.4, 25.8, "Circle"),
    ("DS4_Face_Button.png", 322.0, 145.2, 28.4, 25.8, "Cross"),
    ("DS4_Face_Button.png", 292.5, 119.3, 28.4, 25.8, "Square"),

    # D-Pad
    ("DS4_D-PAD_Up.png", 72.9, 99.5, 25.5, 27.8, "Up"),
    ("DS4_D-PAD_Down.png", 71.6, 139.7, 25.5, 29.5, "Down"),
    ("DS4_D-PAD_Left.png", 53.6, 122.0, 31.2, 24.4, "Left"),
    ("DS4_D-PAD_Right.png", 91.5, 120.1, 31.2, 24.1, "Right"),

    # Bumpers & Triggers (Adjusted to shoulder contours)
    ("DS4_L1-Active.png", 56.0, 52.0, 57.0, 28.4, "L1"),
    ("DS4_R1-Active.png", 306.0, 52.0, 57.0, 28.4, "R1"),
    ("DS4_L2-Active.png", 67.0, 26.0, 38.4, 22.9, "L2"),
    ("DS4_R2-Active.png", 314.0, 26.0, 38.1, 22.6, "R2"),

    # Share & Options (Adjusted to pill buttons)
    ("DS4_OptionsShare_Button.png", 126.0, 96.0, 15.2, 24.4, "Share"),
    ("DS4_OptionsShare_Button.png", 279.0, 96.0, 15.2, 24.4, "Options"),

    # Home
    ("DS4_Home_Button.png", 197.5, 165.4, 24.9, 17.2, "Home"),

    # Sticks (resting position)
    ("PS4_LeftStick.png", 122.4, 174.9, 45.2, 40.2, "LeftStick"),
    ("PS4_RightStick.png", 252.4, 174.9, 45.2, 40.2, "RightStick"),
]

scaled_w = 420
scaled_h = int(round(783.0 * scale))
base_scaled = base.resize((scaled_w, scaled_h), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (420, 260), (0, 0, 0, 255))
canvas.paste(base_scaled, (0, int(round(dy))), base_scaled)

for fname, cx, cy, cw, ch, label in ps4_elements_fine:
    p = os.path.join(ps4_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))

canvas.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_ps4_fine.png")
print("Saved test_ps4_fine.png")
