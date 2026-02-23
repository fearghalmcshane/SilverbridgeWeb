# Keycloak Realm Roles for Silverbridge Web

The admin portal and future features use the following **realm roles**. Create them in Keycloak (Realm **silverbridge** → Realm roles → Create role) and assign to users as needed.

| Role       | Description |
|-----------|-------------|
| **Admin** | Full access to the admin portal (dashboard, news, teams, events, settings). |
| **Coach** | Access to coaching tools (e.g. drill designer, player development). Can be granted in addition to or instead of Admin where appropriate. |
| **Player** | Registered player; used for player-specific features (e.g. development tracking, squad view). |

The WebUI authorization policies are:

- **Admin** policy: requires role `Admin` or `Administrator` (existing backend role).
- **Coach** policy: requires `Coach`, `Admin`, or `Administrator`.
- **Player** policy: requires `Player`, `Coach`, `Admin`, or `Administrator`.

To assign realm roles to a user in Keycloak: Users → select user → Role mapping → Assign role → select the realm role.
