#if [ -z "$TRAVIS_PULL_REQUEST" ] || [ "$TRAVIS_PULL_REQUEST" == "false" ]; then
  #if [ "$TRAVIS_BRANCH" == "master" ]; then
    docker login -e="$DOCKER_EMAIL" -u="$DOCKER_USERNAME" -p="$DOCKER_PASSWORD";
    docker tag crunch_web:lastest hartmannr76/crunch-api
    docker push hartmannr76/crunch-api;
  #fi
#fi