# Crunch

### What is it?
---
Crunch is an AB framework running on C# and .NET Core. This is fairly light weight at the moment and only consists of API endpoints to be performing actions.
I intend on giving this a web interface, but at the moment, this is what I've got. This takes some concepts from SeatGeeks Sixpack project, and I've conformed
it to fit more use cases that I've seen at my current company, Zocdoc.

### Why "Crunch"
---
This is using "AB's" and I thought it was a fun play on the muscle "abs", so a common exercise for abs, is a "crunch". Lame, I know; but it was the best I got.

## The API
---
The current endpoints supported in this project are:
*   `POST /api/experiments/v1/tests` - Configures a new or existing experiment (if it already existed). This will increment the experiments version and attempt to clear any existing data for the previous version,
to allow it to be completely independent of other versions.
*   `GET /api/experiments/v1/tests/{test-name}` - Gets the current configuration information for a test given a test name.
*   `GET /api/experiments/v1/participants/{participant-id}/tests/{test-name}` - Gets the variant for a participant. Inside, this will use the cache if we have already gotten a variant, or fetches a new one if the
test version has changed.
*   `GET /api/experiments/v1/participants/{participant-id}/conversions/{goal-name}` - Triggers a goal for the participant. Crunch keeps track of all goals that have been triggered for tests, so will only trigger a single
conversion for any given user/test version. If the version of the test has changed, the user is able to conver that goal for that test again.

## Test configuration
---
I've attempted to make the conversion pretty light weight, so it follows the following example:

```json
{
    "name": "test-name",
    "version": 1,
    "variants": [
        {
            "name": "variant-1",
            "influence": 0.50
        },
        {
            "name": "variant-2",
            "influence": 0.50
        }
    ]
}
```

- `name` - This can be any non-whitespace string you use to identify the test.
- `version` - Right now, you can send this to the server in the `POST`, but Crunch will always set it
to what it determines is appropriate.
- `variants` - Array of `variant` objects that use the following pattern:
  - `name` - Unique non-white name of a variant you can use to recognize what test group a user is in.
  - `influence` - The strength of the variant for traffic. The influences of the variants must add up to `1.0`,
however this is not enforced at the moment.

## Run & Deploy

I currently run and test this in docker containers, and have it using dotnet-watcher polling to periodically recompile
and work with the latest build.

1) Ensure Docker is installed on your machine and that you have Docker toolbelt
2) Run: `docker-compose up` if you like to have a console hooked up, or add `-d` ro run detached.
