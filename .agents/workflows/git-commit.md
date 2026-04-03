---
description: Workflow for regular and atomic commits in the RevPay project
---

To maintain a clean and descriptive project history, follow these steps for regular commits:

1. **Check Status**
   Check what files have changed:
   ```bash
   git status
   ```

2. **Stage Changes**
   Stage specific files that belong to a single logical change:
   ```bash
   git add [file1] [file2]
   ```
   *Tip: Use `git add -p` to stage specific parts of a file.*

3. **Commit with Conventional Messages**
   Use a clear, concise message following the Conventional Commits standard:
   ```bash
   git commit -m "[type]: [description]"
   ```
   **Common Types:**
   - `feat`: A new feature
   - `fix`: A bug fix
   - `docs`: Documentation only changes
   - `style`: Changes that do not affect the meaning of the code (white-space, formatting, missing semi-colons, etc)
   - `refactor`: A code change that neither fixes a bug nor adds a feature
   - `perf`: A code change that improves performance
   - `test`: Adding missing tests or correcting existing tests
   - `chore`: Changes to the build process or auxiliary tools and libraries such as documentation generation

4. **Verify and Push**
   Check your commit and push to the remote repository:
   ```bash
   git log -n 1 --oneline
   git push origin main
   ```
