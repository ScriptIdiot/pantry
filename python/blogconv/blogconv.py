#!/usr/bin/env python3
import requests, pathlib, argparse, regex
from urllib.parse import urlparse
from markdownify import markdownify
from bs4 import BeautifulSoup


def get_blog_html(url: str) -> str:
    """ get the raw html of the blog post """
    try:
        response = requests.get(url)
        if response.ok:
            return response.text
        else:
            return None
    except Exception as e:
        print(f"[!] Failed to get blog html: {str(e)}")
        return None


def get_images(html: str, url: str, blog_name: str) -> bool:
    """from the raw html, download the images"""
    soup = BeautifulSoup(html, "html.parser")
    imgs: list[str] = soup.findAll("img")
    for img in imgs:
        if not img:
            continue
        src: str = img["src"]
        image_name: str = ""
        if "http" not in src:
            if src.startswith("/"):
                p = urlparse(url)
                scheme = p.scheme
                domain = p.netloc
                if src.startswith("/"):
                    src = f"{scheme}://{domain}{src}"
                else:
                    src = f"{scheme}://{domain}/{src}"
                p = urlparse(src)
                image_name = pathlib.Path(p.path).name
        else:
            image_name = pathlib.Path(src).name

        if image_name == "":
            return False

        r = regex.compile(r".*\.\w+")

        matches = r.findall(image_name)
        if not matches:
            continue
        else:
            if len(image_name) < 25:
                blog_name = str(blog_name)
                if str(blog_name).endswith(".md"):
                    image_name = f"{str(blog_name).replace('.md','')}-{matches[0]}"
                else:
                    image_name = f"{str(blog_name)}-{matches[0]}"
            else:
                image_name = matches[0]

        try:
            response = requests.get(src)
            if response.ok:
                content = response.content
                with open(image_name, "wb") as f:
                    f.write(content)
            else:
                return False
        except Exception as e:
            print(f"[!] Failed to get image: {str(e)}")
            return False


def get_blog_name(url: str) -> str:
    """get the name of the blog"""
    path = urlparse(url).path
    if path.endswith("/"):
        return path.split("/")[-2] + ".md"
    else:
        return path.split("/")[-1] + ".md"


def main() -> None:
    """main!"""

    parser = argparse.ArgumentParser(description="Parse blog to local markdown")
    parser.add_argument("-u", "--url", help="URL to parse", required=True)
    parser.add_argument(
        "-o",
        "--output",
        help="Directory to create for output",
        required=True,
    )
    args = parser.parse_args()
    p = pathlib.Path(args.output)

    try:
        p.mkdir(parents=True, exist_ok=True)
    except Exception as e:
        print(f"[!] Failed to create {args.output}: {str(e)}")
        return

    if p == None:
        return

    html: str = get_blog_html(args.url)
    if html == None:
        return

    blog_name = get_blog_name(args.url)
    markdown = markdownify(html)
    with open(p / blog_name, "w") as f:
        f.write(markdown)
    get_images(html, args.url, p / blog_name)


if __name__ == "__main__":
    main()
