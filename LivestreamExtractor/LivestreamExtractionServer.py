
"""
Simple HTTP server
Only handles POST requests
Looks for encrypted crunchyroll url,
unencrypts it with streamlink and sends back
the unencrypted HLS URL.
"""
import sys
import json
import re, urllib
from BaseHTTPServer import BaseHTTPRequestHandler, HTTPServer
from cgi import parse_header, parse_multipart
from urlparse import parse_qs
from streamlink import Streamlink, PluginError, NoPluginError



class Server(BaseHTTPRequestHandler):
    """Simple HTTP server class"""
    def _set_headers(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', 'http://beta.sync.theater/')
        
        self.end_headers()

    def do_GET(self):
        """Process POST request"""
        cr = parse_header(self.headers.getheader('x-cr-url'))
        print cr
        cr = cr[0]
        if cr is not None and is_valid_url(cr) is not False and is_valid_url(cr) is not None:
            enc_url = cr
            
            post_url = extract_stream(enc_url)
            self._set_headers()
            self.wfile.write(enc_url)

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
    url = stream_url[0]

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


def run(server_class=HTTPServer, handler_class=Server, port=8080):
    """Call to initialize and run the server"""
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    print 'running livestream extractor on port {0}'.format(port)
    httpd.serve_forever()

def main():
    """Main function"""
    from sys import argv

    if len(argv) == 2:
        run(port=int(argv[1]))
    else:
        run()

if __name__ == '__main__':
    main()
