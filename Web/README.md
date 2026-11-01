# E-invitation

Wedding invitation frontend built with Vue 3 and Vite.

## Recommended IDE Setup

[VS Code](https://code.visualstudio.com/) + [Vue (Official)](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Recommended Browser Setup

- Chromium-based browsers (Chrome, Edge, Brave, etc.):
  - [Vue.js devtools](https://chromewebstore.google.com/detail/vuejs-devtools/nhdogjmejiglipccpnnnanhbledajbpd)
  - [Turn on Custom Object Formatter in Chrome DevTools](http://bit.ly/object-formatters)
- Firefox:
  - [Vue.js devtools](https://addons.mozilla.org/en-US/firefox/addon/vue-js-devtools/)
  - [Turn on Custom Object Formatter in Firefox DevTools](https://fxdx.dev/firefox-devtools-custom-object-formatters/)

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vite.dev/config/).

## Project Setup

```sh
npm install
```

### Compile and Hot-Reload for Development

```sh
npm run dev
```

### Type-Check, Compile and Minify for Production

```sh
npm run build
```

### Lint with [ESLint](https://eslint.org/)

```sh
npm run lint
```

## Admin portal

The public invitation is available at `/`; the admin sign-in and dashboard are
available at `/admin/login` and `/admin/dashboard`.

The dashboard loads guests from the Admin-only `GET /api/guests` endpoint and
creates invitations with `POST /api/guests`. It shows each guest's RSVP status
and personal invitation URL, which can be copied and shared. Guests can be
removed using the Admin-only `DELETE /api/guests/{id}` endpoint. The public
`/invite/:token` page loads the guest name from `GET /api/guests/{token}`.
Invitation QR codes are generated locally in the browser and can be downloaded
as PNG files; guest tokens are not sent to an external QR service.

The frontend sends authentication requests through the Vite `/api` proxy, so
the API cookie remains same-origin in development. The proxy targets the API
HTTP launch profile at `http://localhost:5183`. To access Vite from another
device on your local network, start it with:

```sh
npm run dev -- --host 0.0.0.0
```

Start the API with its HTTP launch profile:

```sh
dotnet run --launch-profile http
```

Development uses a non-secure session cookie so it works over the local HTTP
address. Use HTTPS in production; production cookies remain secure-only. Do not
use the HTTP development server on an untrusted network.

From the `api` project, apply its migrations and create an administrator:

```sh
dotnet ef database update
dotnet run -- --create-admin
```

Then sign in with that account. Set `Invitation:BaseUrl` in the API
configuration to the public frontend URL so generated invitation links point
to the correct site.
