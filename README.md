# LUMEN Google Fit Module

This module queries the Google Fit REST API for step data and stores it in a PostgreSQL database.
It records both **daily totals** and **hourly breakdowns**, keeping yesterday's record up to date
(Google Fit may backfill data overnight) and refreshing today's hourly buckets on every run.

## Grafana dashboard

Import `grafana/dashboard.json` into your Grafana instance and point it at the PostgreSQL
datasource to get:

- **Steps today / yesterday** stat panels with green/yellow/red thresholds (10 000 / 5 000)
- **Daily step history** time-series for the selected time range
- **Hourly steps** time-series
- **Today's steps by hour** bar chart

## Configuration

The module reads two `ConfigEntry` values from the Lumen config store:

| Key            | Description                                                                 |
|----------------|-----------------------------------------------------------------------------|
| `API_KEY`      | Google Cloud API key with the Fitness API enabled                           |
| `ACCESS_TOKEN` | OAuth2 access token for the Google account whose fitness data is queried    |

> **Note:** The Google Fit Fitness REST API requires a user-scoped OAuth2 access token even
> when supplying an API key. The API key identifies your GCP project; the access token grants
> access to a specific user's data. See the
> [Google Fit REST API docs](https://developers.google.com/fit/rest/v1/get-started) for how to
> obtain both credentials.

## Database schema

All tables live in the `googlefit` PostgreSQL schema.

| Table          | Primary key  | Columns                 |
|----------------|--------------|-------------------------|
| `DailySteps`   | `Date`       | `Date`, `Steps`         |
| `HourlySteps`  | `HourStart`  | `HourStart`, `Steps`    |

## Scheduling

| Flag  | Trigger                              |
|-------|--------------------------------------|
| `API` | Every hour at `HH:00:00` (`minute == 0 && second == 0`) |
| `UI`  | Not used                             |
