# if [ -z "$TRAVIS_PULL_REQUEST" ] || [ "$TRAVIS_PULL_REQUEST" == "false" ]; then
  # if [ "$TRAVIS_BRANCH" == "master" ]; then
    # docker login -e="$DOCKER_EMAIL" -u="$DOCKER_USERNAME" -p="$DOCKER_PASSWORD" registry.heroku.com
    
    # Build and push
    # docker build -t $IMAGE_NAME .
    # heroku plugins:install heroku-container-registry
    #heroku container:login
    # we need to login here somehow
    docker-compose -f docker-compose.heroku.yml build
    heroku plugins:install heroku-container-registry
    docker login --email=_ --username=_ --password=$HEROKU_API_KEY registry.heroku.com
    echo "Pushing $IMAGE_NAME:latest"
    docker tag crunch_web "$HEROKU_REMOTE_URL/$HEROKU_APP/web"
    docker push "$HEROKU_REMOTE_URL/$HEROKU_APP/web"
    echo "Pushed $IMAGE_NAME:latest"
  # else
    # echo "Skipping deploy because branch is not 'master'"
#   fi
# else
#   echo "Skipping deploy because it's a pull request"
# fi