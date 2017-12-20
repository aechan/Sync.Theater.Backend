
"""
Simple HTTP server
Only handles GET requests
Uses really basic, super insecure auth
Looks for encrypted crunchyroll url,
unencrypts it with streamlink and sends back
the unencrypted HLS URL.
"""
import sys
import json
import re
import urllib
import base64
from BaseHTTPServer import BaseHTTPRequestHandler, HTTPServer
from cgi import parse_header, parse_multipart
from urlparse import parse_qs
from streamlink import Streamlink, PluginError, NoPluginError


class ServerHandler(BaseHTTPRequestHandler):
    key = ""
    ''' HTTP request handler class. '''
    def do_HEAD(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Content-type', 'text/html')
        self.end_headers()
    
    def do_OPTIONS(self):
        self.send_response(200, "ok")
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, OPTIONS')
        self.send_header("Access-Control-Allow-Headers", "X-Requested-With")
        self.send_header("Access-Control-Allow-Headers", "Content-Type")
        self.send_header("Access-Control-Allow-Headers", "Authorization")
        self.send_header("Access-Control-Allow-Headers", "x-cr-url")
        self.end_headers()

    def do_AUTHHEAD(self):
        self.send_response(401)
        self.send_header('WWW-Authenticate', 'Basic realm=\"/\"')
        self.send_header('Content-type', 'text/html')
        self.end_headers()

    def do_GET(self):
        if self.headers.getheader('Authorization') is None:
            self.do_AUTHHEAD()
            self.wfile.write('no auth header received')
            pass
        elif self.headers.getheader('Authorization') == 'Basic '+self.key:
            cr = self.headers.getheader('x-cr-url')
            if cr is not None and is_valid_url(cr) is not False and is_valid_url(cr) is not None:
                enc_url = cr
                post_url = extract_stream(enc_url)
                self.do_HEAD()
                self.wfile.write(post_url)
            else:
                self.send_error(500, "Livestream URL is invalid or this server cannot process it.")
            pass
        else:
            self.do_AUTHHEAD()
            self.wfile.write(self.headers.getheader('Authorization'))
            self.wfile.write(' not authenticated')
            pass


def is_valid_url(url):
    import re
    regex = re.compile(
        r'^https?://'  # http:// or https://
        r'(?:(?:[A-Z0-9](?:[A-Z0-9-]{0,61}[A-Z0-9])?\.)+[A-Z]{2,6}\.?|'  # domain...
        r'localhost|'  # localhost...
        r'\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})' # ...or ip
        r'(?::\d+)?'  # optional port
        r'(?:/?|[/?]\S+)$', re.IGNORECASE)
    return url is not None and regex.search(url)


def extract_stream(stream_url):
    """extracts the stream url from the encrypted url"""
    url = stream_url

    # Create the Streamlink session
    streamlink = Streamlink()

    #stream quality
    quality = "best"
    # Enable logging
    streamlink.set_loglevel("info")
    streamlink.set_logoutput(sys.stdout)

    # Attempt to fetch streams
    try:
        streams = streamlink.streams(url)
    except NoPluginError:
        exit("Streamlink is unable to handle the URL '{0}'".format(url))
    except PluginError as err:
        exit("Plugin error: {0}".format(err))

    if not streams:
        exit("No streams found on URL '{0}'".format(url))

    # Look for specified stream
    if quality not in streams:
        exit("Unable to find '{0}' stream on URL '{1}'".format(quality, url))

    # We found the stream
    stream = streams[quality]

    return stream.url


def run(authkey, server_class=HTTPServer, handler_class=ServerHandler, port=8080):
    """Call to initialize and run the server"""
    handler_class.key = authkey
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    httpd.serve_forever()


def main():
    """Main function"""
    from sys import argv

    if len(sys.argv) < 2:
        sys.exit()

    key = base64.b64encode(sys.argv[1])
    
    run(key)

if __name__ == '__main__':
    main()
