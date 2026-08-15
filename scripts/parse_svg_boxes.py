import xml.etree.ElementTree as ET
import os
import re
import numpy as np
from matplotlib.path import Path
import matplotlib.patches as patches

# Load the SVG file:
svg_press = r'C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox Wireless Controller Images\Default Theme\Theme Assets\Xbox Series X Active Presses\Xbox Series X Active Press.svg'
tree = ET.parse(svg_press)
root = tree.getroot()

# SVG viewBox or dimensions
print("Root viewBox:", root.attrib.get('viewBox'))
print("Root width/height:", root.attrib.get('width'), root.attrib.get('height'))

# Let's inspect the elements and their transforms and paths
from matplotlib.path import Path
from matplotlib.transforms import Affine2D

def parse_svg_transform(transform_str):
    if not transform_str:
        return Affine2D()
    t = Affine2D()
    # handle translate(x, y) or matrix(a,b,c,d,e,f)
    for part in re.finditer(r'([a-zA-Z]+)\s*\(([^)]+)\)', transform_str):
        name = part.group(1)
        args = [float(x.strip()) for x in re.split(r'[\s,]+', part.group(2).strip()) if x.strip()]
        if name == 'translate':
            if len(args) == 1:
                t.translate(args[0], 0)
            elif len(args) == 2:
                t.translate(args[0], args[1])
        elif name == 'matrix':
            if len(args) == 6:
                # SVG matrix(a,b,c,d,e,f)
                a, b, c, d, e, f = args
                mat = Affine2D.from_values(a, b, c, d, e, f)
                t = mat + t
        elif name == 'scale':
            if len(args) == 1:
                t.scale(args[0], args[0])
            elif len(args) == 2:
                t.scale(args[0], args[1])
    return t

# Let's find all paths and their bounding boxes
from matplotlib.path import Path

# Custom minimal SVG path parser for standard SVG commands (M, C, S, L, H, V, Z, etc.)
def get_path_points(d_str):
    # Extracts all coordinate points from path d string
    # Replace commands with spaces and extract numbers
    nums = [float(x) for x in re.findall(r'[-+]?(?:\d*\.\d+|\d+)(?:[eE][-+]?\d+)?', d_str)]
    # Pair up numbers as (x, y)
    pts = []
    for i in range(0, len(nums) - 1, 2):
        pts.append((nums[i], nums[i+1]))
    return np.array(pts) if pts else np.empty((0, 2))

# Let's iterate all elements and accumulate transforms
def process_element(elem, parent_transform):
    elem_transform = parse_svg_transform(elem.attrib.get('transform', ''))
    total_transform = elem_transform + parent_transform
    
    lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    eid = elem.attrib.get('id', '')
    tag = elem.tag.split('}')[-1]
    
    if tag == 'path':
        d = elem.attrib.get('d', '')
        if d:
            pts = get_path_points(d)
            if len(pts) > 0:
                t_pts = total_transform.transform(pts)
                min_x, min_y = t_pts.min(axis=0)
                max_x, max_y = t_pts.max(axis=0)
                w = max_x - min_x
                h = max_y - min_y
                print(f"Path: {eid:18s} ({lbl:20s}) -> bbox: x={min_x:.1f}, y={min_y:.1f}, w={w:.1f}, h={h:.1f}")
    elif tag == 'ellipse' or tag == 'circle':
        cx = float(elem.attrib.get('cx', 0))
        cy = float(elem.attrib.get('cy', 0))
        rx = float(elem.attrib.get('rx', elem.attrib.get('r', 0)))
        ry = float(elem.attrib.get('ry', elem.attrib.get('r', 0)))
        pts = np.array([[cx - rx, cy - ry], [cx + rx, cy + ry]])
        t_pts = total_transform.transform(pts)
        min_x, min_y = t_pts.min(axis=0)
        max_x, max_y = t_pts.max(axis=0)
        w = max_x - min_x
        h = max_y - min_y
        print(f"Ellipse: {eid:15s} ({lbl:20s}) -> bbox: x={min_x:.1f}, y={min_y:.1f}, w={w:.1f}, h={h:.1f}")
    elif tag == 'rect':
        x = float(elem.attrib.get('x', 0))
        y = float(elem.attrib.get('y', 0))
        w = float(elem.attrib.get('width', 0))
        h = float(elem.attrib.get('height', 0))
        pts = np.array([[x, y], [x + w, y + h]])
        t_pts = total_transform.transform(pts)
        min_x, min_y = t_pts.min(axis=0)
        max_x, max_y = t_pts.max(axis=0)
        print(f"Rect: {eid:18s} ({lbl:20s}) -> bbox: x={min_x:.1f}, y={min_y:.1f}, w={max_x-min_x:.1f}, h={max_y-min_y:.1f}")
        
    for child in elem:
        process_element(child, total_transform)

process_element(root, Affine2D())
