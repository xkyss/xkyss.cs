
################ @Model.HostIp

# deploy
echo ''
echo 'download [deploy] configs'
New-Item .@Model.WorkDir/deploy/ -ItemType Directory -ea 0

scp -r @Model.Username@@@Model.HostIp:@Model.WorkDir/deploy/* .@Model.WorkDir/deploy/

# docker
echo ''
echo 'download [docker] configs'
New-Item .@Model.WorkDir/docker/ -ItemType Directory -ea 0

# scp -r @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/* .@Model.WorkDir/docker/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/.env .@Model.WorkDir/docker/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/*.yml .@Model.WorkDir/docker/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/*.sh .@Model.WorkDir/docker/

# base-env
echo ''
echo 'download [base-env] configs'
New-Item .@Model.WorkDir/docker/base-env -ItemType Directory -ea 0

scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/base-env/.env .@Model.WorkDir/docker/base-env/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/base-env/* .@Model.WorkDir/docker/base-env/

# mysql
echo ''
echo 'download [mysql] configs'
New-Item .@Model.WorkDir/docker/mysql/conf -ItemType Directory -ea 0
New-Item .@Model.WorkDir/docker/mysql/init -ItemType Directory -ea 0

scp -r @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/mysql/* .@Model.WorkDir/docker/mysql/

# redis
echo ''
echo 'download [redis] configs'
New-Item .@Model.WorkDir/docker/redis -ItemType Directory -ea 0

scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/redis/* .@Model.WorkDir/docker/redis/

# elk
echo ''
echo 'download [elk] configs'
New-Item .@Model.WorkDir/docker/elk/logstash/pipeline -ItemType Directory -ea 0
New-Item .@Model.WorkDir/docker/elk/logstash/config -ItemType Directory -ea 0

scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/elk/* .@Model.WorkDir/docker/elk/
scp -r @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/elk/logstash/config/* .@Model.WorkDir/docker/elk/logstash/config/
scp -r @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/elk/logstash/pipeline/* .@Model.WorkDir/docker/elk/logstash/pipeline/

# sample
#
# ignore:
# - ar
#    |- mlcache-ar-sample.jar
# - sample
#    |- mlcache-sample.jar
#    |- dist
#
echo ''
echo 'download [sample] configs'
New-Item .@Model.WorkDir/docker/sample/nginx -ItemType Directory -ea 0
New-Item .@Model.WorkDir/docker/sample/ar -ItemType Directory -ea 0

scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample/.env .@Model.WorkDir/docker/sample/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample/* .@Model.WorkDir/docker/sample/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample/nginx/* .@Model.WorkDir/docker/sample/nginx/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample/ar/*.yml .@Model.WorkDir/docker/sample/ar/

echo ''
echo 'download [sample-cpp] configs'
New-Item .@Model.WorkDir/docker/sample-cpp/conf -ItemType Directory -ea 0

scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample-cpp/conf/* .@Model.WorkDir/docker/sample-cpp/conf/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample-cpp/.env .@Model.WorkDir/docker/sample-cpp/
scp @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample-cpp/* .@Model.WorkDir/docker/sample-cpp/