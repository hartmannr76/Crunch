# if [ -z "$TRAVIS_PULL_REQUEST" ] || [ "$TRAVIS_PULL_REQUEST" == "false" ]; then
  # if [ "$TRAVIS_BRANCH" == "master" ]; then
    pip install --user awscli
    export PATH=$PATH:$HOME/.local/bin
    eval $(aws ecr get-login --region $AWS_DEFAULT_REGION)
    
    # Build and push
    # docker build -t $IMAGE_NAME .
    echo "Pushing $IMAGE_NAME:latest"
    docker tag $IMAGE_NAME:latest "$REMOTE_IMAGE_URL:latest"
    docker push "$REMOTE_IMAGE_URL:latest"
    echo "Pushed $IMAGE_NAME:latest"
  # else
    # echo "Skipping deploy because branch is not 'master'"
#   fi
# else
#   echo "Skipping deploy because it's a pull request"
# fi