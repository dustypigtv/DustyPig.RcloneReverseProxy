[![Auto-Build](https://github.com/dustypigtv/DustyPig.RcloneReverseProxy/actions/workflows/auto_build.yml/badge.svg)](https://github.com/dustypigtv/DustyPig.RcloneReverseProxy/actions/workflows/auto_build.yml)&nbsp;&nbsp;[![Release](https://github.com/dustypigtv/DustyPig.RcloneReverseProxy/actions/workflows/release.yml/badge.svg)](https://github.com/dustypigtv/DustyPig.RcloneReverseProxy/actions/workflows/release.yml)


Simple server, meant to sit behind an nginx reverse proxy, to validate file requests

## Example nginx config:

```
server {

	listen 80;
	listen [::]:80;
	server_name example.com;

	location / {
		auth_request /auth;
		auth_request_set rclone_path $upstream_http_rclone_path;
		proxy_buffering off;
		proxy_pass http://127.0.0.1:8080/$rclone_path;
		proxy_pass_request_body off;
		proxy_set_header Content-Length "";
	}
	
	location = /auth {
		internal;
		proxy_pass http://localhost:7890/tokens;
		proxy_pass_request_body off;
		proxy_set_header Content-Length "";
		proxy_set_header X-Original-URI $request_uri;
    }
}
```