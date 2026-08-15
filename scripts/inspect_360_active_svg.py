import xml.etree.ElementTree as ET
import re

svg_360 = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Theme SVG\Theme Assets\SVG\Xbox 360 Active Buttons.svg"
tree = ET.parse(svg_360)
root = tree.getroot()

print("viewBox:", root.attrib.get('viewBox'))
print("width/height:", root.attrib.get('width'), root.attrib.get('height'))

for elem in root.iter():
    lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    eid = elem.attrib.get('id', '')
    tag = elem.tag.split('}')[-1]
    if lbl or '360' in eid or 'btn' in eid.lower():
        print(f"Tag: {tag:10s} ID: {eid:25s} Label: {lbl}")
