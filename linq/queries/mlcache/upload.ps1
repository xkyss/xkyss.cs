################ @Model.HostIp

# deploy
echo ''
echo 'upload [deploy] configs'
scp -r .@Model.WorkDir/deploy/* @Model.Username@@@Model.HostIp:@Model.WorkDir/deploy/

# docker
echo ''
echo 'upload [docker] configs'
# scp -r .@Model.WorkDir/docker/* @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/
scp .@Model.WorkDir/docker/.env @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/
scp .@Model.WorkDir/docker/*.yml @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/
scp .@Model.WorkDir/docker/*.sh @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/

# base-env
echo ''
echo 'upload [base-env] configs'
scp .@Model.WorkDir/docker/base-env/.env @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/base-env/
scp .@Model.WorkDir/docker/base-env/* @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/base-env/

# mysql
echo ''
echo 'upload [mysql] configs'
scp -r .@Model.WorkDir/docker/mysql/* @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/mysql/


# redis
echo ''
echo 'upload [redis] configs'
scp -r .@Model.WorkDir/docker/redis/* @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/redis/

# elk
echo ''
echo 'upload [elk] configs'
scp -r .@Model.WorkDir/docker/elk/* @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/elk/

# sample
echo ''
echo 'upload [sample] configs'
scp -r .@Model.WorkDir/docker/sample/.env @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample/
scp -r .@Model.WorkDir/docker/sample/* @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample/

# sample-cpp
echo ''
echo 'upload [sample-cpp] configs'
scp -r .@Model.WorkDir/docker/sample-cpp/conf/* @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample-cpp/conf/
scp -r .@Model.WorkDir/docker/sample-cpp/.env @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample-cpp/
scp -r .@Model.WorkDir/docker/sample-cpp/* @Model.Username@@@Model.HostIp:@Model.WorkDir/docker/sample-cpp/
