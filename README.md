# web-cat-asp

Create docker hub repository - publish
```
docker build -t web-cat-asp-api . 
docker push komarandrii/web-cat-asp-api:latest

docker run -it --rm -p 4531:8080 --name web-cat-asp_container web-cat-asp-api
docker run -d --restart=always --name web-cat-asp_container -p 4531:8080 web-cat-asp-api
docker run -d --restart=always -v d:/volumes/web-cat-asp/images:/app/uploading --name web-cat-asp_container -p 4531:8080 web-cat-asp-api
docker run -d --restart=always -v /volumes/web-cat-asp/images:/app/uploading --name web-cat-asp_container -p 4531:8080 web-cat-asp-api
docker ps -a
docker stop web-cat-asp_container
docker rm web-cat-asp_container

docker images --all
docker rmi web-cat-asp-api

docker login
docker tag web-cat-asp-api:latest komarandrii/web-cat-asp-api:latest
docker push komarandrii/web-cat-asp-api:latest

docker pull komarandrii/web-cat-asp-api:latest
docker ps -a
docker run -d --restart=always --name pv212-asp_container -p 4531:8080 komarandrii/web-cat-asp-api

docker run -d --restart=always -v /volumes/web-cat-asp/images:/app/uploading --name web-cat-asp_container -p 4531:8080 komarandrii/web-cat-asp-api


docker pull komarandrii/web-cat-asp-api:latest
docker images --all
docker ps -a
docker stop web-cat-asp_container
docker rm web-cat-asp_container
docker run -d --restart=always --name web-cat-asp_container -p 4531:8080 komarandrii/web-cat-asp-api
```

```nginx options /etc/nginx/sites-available/default
server {
    server_name   web-catapi.itstep.click *.web-catapi.itstep.click;
    client_max_body_size 200M;
    location / {
       proxy_pass         http://localhost:4531;
       proxy_http_version 1.1;
       proxy_set_header   Upgrade $http_upgrade;
       proxy_set_header   Connection keep-alive;
       proxy_set_header   Host $host;
       proxy_cache_bypass $http_upgrade;
       proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
       proxy_set_header   X-Forwarded-Proto $scheme;
    }
}


sudo systemctl restart nginx
certbot
```
