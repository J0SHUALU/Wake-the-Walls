# Contributing

## Branches

- `main`: release merges only.
- `develop`: the working branch. Every feature merges here.
- `feature/name-task`: your work, for example `feature/remy-tracking-manager`.

## Workflow

1. `git checkout develop && git pull`
2. `git checkout -b feature/yourname-task`
3. Make small, focused commits and push at least once a day.
4. Open a PR into `develop`. One other member reviews it.
5. Merge with **Create a merge commit** (no squash) so everyone's commits stay in the history.
6. Delete the branch after it merges.

Always commit from your own machine and your own GitHub account. Never commit for someone else.

## Commit messages

Conventional Commits, short and in the present tense:

```
feat: add tracking loss grace period
fix: stop info panel opening twice
chore: update iOS player settings
docs: add mural 3 story
refactor: move fade logic into Tween
style: apply UI colours to completion screen
```

Write commits, PRs and comments in your own words. No generated-by notes, tool attribution or co-author trailers.

## Unity tips

- Open the project with exactly Unity 6000.4.7f1.
- Always commit the `.meta` file next to every asset.
- Two people should not edit the same scene at the same time. Build your parts as prefabs and drop them into `Main.unity` in a small separate commit.
- Pull before you open Unity, and close Unity before you switch branches.

## Assets from the internet

Only use **CC0** assets (for example Kenney, Poly Haven, ambientCG, CC0 sounds on Freesound, OFL fonts from Google Fonts). Put them in `Assets/ThirdParty/` and keep the license file that comes with them.
