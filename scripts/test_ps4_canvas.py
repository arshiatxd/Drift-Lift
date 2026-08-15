from PIL import Image
import os

ps4_base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4\PS4_Base.png"
ps4_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4"

base = Image.open(ps4_base_path).convert("RGBA")
scale = 420.0 / 1466.0
dy = (260.0 - 783.0 * scale) / 2.0
dx = 0.0

# 1466x783 Center coordinates:
# Face Buttons:
tri_cx, tri_cy = 1173.7, 311.8
cir_cx, cir_cy = 1273.0, 401.0
crs_cx, crs_cy = 1173.6, 489.5
sqr_cx, sqr_cy = 1070.3, 399.1

# D-Pad:
d_up_cx, d_up_cy = 298.9, 333.6
d_dn_cx, d_dn_cy = 294.4, 476.8
d_lf_cx, d_lf_cy = 241.5, 406.0
d_rt_cx, d_rt_cy = 374.0, 399.1

# Share & Options:
sh_cx, sh_cy = 496.0, 304.5
opt_cx, opt_cy = 969.0, 304.5

# L1, L2, R1, R2:
l1_cx, l1_cy = 355.0, 208.0
r1_cx, r1_cy = 1110.0, 208.0
l2_cx, l2_cy = 355.0, 118.0
r2_cx, r2_cy = 1110.0, 118.0

# Home:
home_cx, home_cy = 733.0, 545.0

ps4_elements = [
    # Face Buttons
    ("DS4_Face_Button.png", tri_cx - 99/2, tri_cy - 90/2, 99, 90, "Triangle"),
    ("DS4_Face_Button.png", cir_cx - 99/2, cir_cy - 90/2, 99, 90, "Circle"),
    ("DS4_Face_Button.png", crs_cx - 99/2, crs_cy - 90/2, 99, 90, "Cross"),
    ("DS4_Face_Button.png", sqr_cx - 99/2, sqr_cy - 90/2, 99, 90, "Square"),

    # D-Pad
    ("DS4_D-PAD_Up.png", d_up_cx - 89/2, d_up_cy - 97/2, 89, 97, "Up"),
    ("DS4_D-PAD_Down.png", d_dn_cx - 89/2, d_dn_cy - 103/2, 89, 103, "Down"),
    ("DS4_D-PAD_Left.png", d_lf_cx - 109/2, d_lf_cy - 85/2, 109, 85, "Left"),
    ("DS4_D-PAD_Right.png", d_rt_cx - 109/2, d_rt_cy - 84/2, 109, 84, "Right"),

    # Bumpers & Triggers
    ("DS4_L1-Active.png", l1_cx - 199/2, l1_cy - 99/2, 199, 99, "L1"),
    ("DS4_R1-Active.png", r1_cx - 199/2, r1_cy - 99/2, 199, 99, "R1"),
    ("DS4_L2-Active.png", l2_cx - 134/2, l2_cy - 80/2, 134, 80, "L2"),
    ("DS4_R2-Active.png", r2_cx - 133/2, r2_cy - 79/2, 133, 79, "R2"),

    # Share & Options
    ("DS4_OptionsShare_Button.png", sh_cx - 53/2, sh_cy - 85/2, 53, 85, "Share"),
    ("DS4_OptionsShare_Button.png", opt_cx - 53/2, opt_cy - 85/2, 53, 85, "Options"),

    # Home
    ("DS4_Home_Button.png", home_cx - 87/2, home_cy - 60/2, 87, 60, "Home"),
]

scaled_w = 420
scaled_h = int(round(783.0 * scale))
base_scaled = base.resize((scaled_w, scaled_h), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (420, 260), (0, 0, 0, 255))
canvas.paste(base_scaled, (0, int(round(dy))), base_scaled)

for fname, bx, by, bw, bh, label in ps4_elements:
    cx = dx + bx * scale
    cy = dy + by * scale
    cw = bw * scale
    ch = bh * scale
    p = os.path.join(ps4_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))
    print(f"[{label:10s}] {fname:25s} -> Canvas.Left=\"{cx:5.1f}\" Canvas.Top=\"{cy:5.1f}\" Width=\"{cw:4.1f}\" Height=\"{ch:4.1f}\"")

canvas.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_ps4_canvas.png")
