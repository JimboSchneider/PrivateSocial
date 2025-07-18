#!/bin/sh

# Replace the API service URL in nginx config with the actual service URL
if [ ! -z "$services__apiservice__https__0" ]; then
    API_URL="$services__apiservice__https__0"
elif [ ! -z "$services__apiservice__http__0" ]; then
    API_URL="$services__apiservice__http__0"
else
    API_URL="http://apiservice"
fi

# Update nginx configuration with the actual API URL
sed -i "s|http://apiservice|$API_URL|g" /etc/nginx/conf.d/default.conf

# Start nginx
nginx -g 'daemon off;'