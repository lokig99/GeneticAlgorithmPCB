#!/bin/python3

from PIL import Image, ImageDraw, ImageFont
from typing import Any, List, Tuple
import json
import sys
import cv2
import io
import os
import numpy as np

ROOT = os.path.abspath(os.path.dirname(__file__))

SCALE = 25
BORDER = 10
FPS = 10
PATH_TRANSPARENCY = 'FF'  # transparency in hexadecimal format (00..FF)
FONT_SIZE = max(16, SCALE // 2)  # in px
FONT = ImageFont.truetype("FreeMonoBold.ttf", size=FONT_SIZE)

COLOR_PALETTE_8 = [
    '#191970',  # midnightblue
    '#006400',  # darkgreen
    '#ff0000',  # red
    '#ffd700',  # gold
    '#00ff00',  # lime
    '#00ffff',  # aqua
    '#ff00ff',  # fuchsia
    '#ffb6c1',  # lightpink
]

COLOR_PALETTE_16 = [
    '#2f4f4f',  # darkslategray
    '#800000',  # maroon
    '#006400',  # darkgreen
    '#00008b',  # darkblue
    '#ff0000',  # red
    '#ffa500',  # orange
    '#ffff00',  # yellow
    '#00ff00',  # lime
    '#00fa9a',  # mediumspringgreen
    '#00ffff',  # aqua
    '#0000ff',  # blue
    '#ff00ff',  # fuchsia
    '#1e90ff',  # dodgerblue
    '#f0e68c',  # khaki
    '#ff1493',  # deeppink
    '#ffb6c1',  # lightpink
]


class BoardDrawer:
    def __init__(self, solution: dict, label: str = '') -> None:
        self.width = solution['board'][0]
        self.height = solution['board'][1]
        self.points = solution['points']
        self.paths = solution['paths']
        self.text = label

        self.__create_plane()
        self.__create_board()

    def __image_cords(self, x: int, y: int) -> Tuple[int, int]:
        im_x = x * SCALE + BORDER + self.__board_dim[0][0]
        im_y = y * SCALE + BORDER + self.__board_dim[0][1]
        return im_x, im_y

    def __draw_text(self, xy: Tuple[int, int], text: str):
        self.__draw.text((xy), text, font=FONT)

    def __create_plane(self):
        min_width = int(FONT.getlength(self.text) +
                        2 * BORDER) if self.text else 0

        inner_width, inner_height = (
            self.width - 1) * SCALE + 3 * BORDER, (self.height - 1) * SCALE + 3 * BORDER

        im_dim = (max(min_width, inner_width) + BORDER,
                  inner_height + BORDER + (FONT_SIZE if self.text else 0))

        self.__board_offset = max(0, min_width - inner_width) // 2

        self.__board_dim = ((BORDER + self.__board_offset,
                             (FONT_SIZE if self.text else 0) + BORDER),
                            (inner_width + self.__board_offset,
                             inner_height + (FONT_SIZE if self.text else 0)))

        self.__img = Image.new('RGB', im_dim)
        self.__draw = ImageDraw.Draw(self.__img)

        # draw outline plane
        self.__draw.rectangle([(0, 0), im_dim], fill=(0, 0, 0))

        # draw inline plane
        self.__draw.rectangle(self.__board_dim,  fill=(84, 84, 84))

        # draw points
        for x in range(self.width):
            for y in range(self.height):
                im_x, im_y = self.__image_cords(x, y)
                self.__draw.rectangle(
                    ((im_x - 1, im_y - 1), (im_x + 1, im_y + 1)), fill=(120, 120, 120))

    def __create_board(self):
        def draw_lines(lines: Tuple[Tuple[int, int], ...], color=(255, 255, 255, 125)):
            line_seq = tuple((self.__image_cords(x, y) for x, y in lines))
            self.__draw.line(line_seq, width=SCALE//4,
                             joint="curve", fill=color)

        def draw_point(x: int, y: int):
            x, y = self.__image_cords(x, y)
            self.__draw.ellipse(((x - SCALE//4, y - SCALE//4),
                                 (x + SCALE // 4, y + SCALE//4)),
                                fill=(255, 255, 255), outline=(0, 0, 0))

        color_palette = COLOR_PALETTE_8 if len(
            self.paths) < 9 else COLOR_PALETTE_16

        # draw paths
        for i, path in enumerate(self.paths):
            color = f'{color_palette[i % len(color_palette)]}{PATH_TRANSPARENCY}'
            draw_lines(path, color=color)

        # draw points
        for x, y in self.points:
            draw_point(x, y)

        # draw text
        self.__draw_text((BORDER, BORDER // 2), self.text)

    def get_image(self) -> Image.Image:
        return self.__img.copy()

    @staticmethod
    def create_label(config: dict) -> str:
        label = ''
        if 'gen' in config:
            label += f'G={config["gen"]} '
        if 'fit' in config:
            label += f'F={round(config["fit"], 3)} '
        return label


def create_video(input: dict, output_path: str):
    board = input['board']
    points = input['points']

    def create_cv2_img(solution: dict, custom_text: str = '') -> Any:
        solution['board'] = board
        solution['points'] = points
        img = BoardDrawer(solution, custom_text).get_image()

        # convert PIL image to byte array
        img_byte_arr = io.BytesIO()
        img.save(img_byte_arr, format='PNG')
        img_byte_arr = img_byte_arr.getvalue()

        # convert byte array to OpenCV2 image
        img = cv2.imdecode(np.frombuffer(img_byte_arr, np.uint8), 1)
        return img

    # create image labels
    labels: List[str] = []
    for solution in input['data']:
        label = BoardDrawer.create_label(solution)
        labels.append(label)

    # find longest label
    max_label_len = max((len(label) for label in labels))

    # equalize label length
    for i, label in enumerate(labels):
        if len(label) < max_label_len:
            tmp = ' ' * (max_label_len - len(label))
            labels[i] = f'{label}{tmp}'

    # initialize cv2 VideoWriter with first image
    img_cv2 = create_cv2_img(input['data'][0], labels[0])
    height, width, _ = img_cv2.shape
    video = cv2.VideoWriter(output_path, 0, FPS, (width, height))
    video.write(img_cv2)

    # create the rest of the video
    for solution, label in zip(input['data'][1:], labels[1:]):
        img = create_cv2_img(solution, label)
        video.write(img)

    cv2.destroyAllWindows()
    video.release()

    os.startfile(output_path)


def main():
    def usage():
        print(
            "usage: generator.py image|video <input (*.json)> [<output> (*.png|*.avi)]")
        return sys.exit(-1)

    argv = sys.argv[1:]
    output_path = ''
    if len(argv) != 3:
        return usage()

    # get .json config
    if not (json_path := argv.pop(1)).lower().endswith('.json'):
        return usage()

    with open(json_path, 'r') as file:
        config = json.load(file)

    # then get the output file path
    output_path = os.path.join(ROOT, argv.pop(1))

    # check the operation mode
    if (mode := argv.pop(0)) == 'video':
        create_video(config, output_path)
    elif mode == 'image':
        label = BoardDrawer.create_label(config)
        img = BoardDrawer(config, label).get_image()
        img.save(output_path)
        os.startfile(output_path)
    else:
        usage()
    
    sys.exit(0)


if __name__ == '__main__':
    main()
