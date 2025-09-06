# Page snapshot

```yaml
- link "PrivateSocial":
  - /url: ""
- navigation:
  - link "Home":
    - /url: /
    - img
    - text: Home
  - link "Posts":
    - /url: /posts
    - img
    - text: Posts
  - separator
  - link "Login":
    - /url: /login
    - img
    - text: Login
  - link "Register":
    - /url: /register
    - img
    - text: Register
- main:
  - link "About":
    - /url: https://learn.microsoft.com/aspnet/core/
  - article:
    - heading "Login" [level=1]
    - alert: Request failed with status code 500
    - text: Username
    - textbox "Username": invaliduser
    - text: Password
    - textbox "Password": invalidpassword
    - button "Login"
    - paragraph:
      - text: Don't have an account?
      - link "Register here":
        - /url: /register
```