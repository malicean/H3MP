# Version Specification (INL)
This version spec used was picked for its ability to notate immediate compatibility between clients and servers, whereas semver (`major.minor.patch`) focuses on backwards compatibility between features. That being said, it is a sequenced version with 3, whole number components and as such **shares notation with semver, but not interpreted as such**.

## Format
`I.N.L`

| Component | Initialism | Represents | Possible cause of change | Result of incompatibility |
| --------: | :--------: | ---------- | ---------------- | ------------------------- |
| Invite    | I          | immediate compatibility of data (often in invites) needed to join a server | invite optimization or invite reformatting | data malformation errors before being able to connect to the server |
| Network   | N          | immediate compatibility of data needed to communicate with networked peers | some feature additions, removals, and optimizations | join prevented by the pre-join version check, and in case of failure, connection refused by the join (networked) version check |
| Local     | L          | update count of non-networked (serverside and/or clientside) changes | default config changes, asset changes, and of course, (most) bug fixes | render only; the world that the newer-version player sees *may* (not will) be different from the world the older-version player sees |

##

## INL vs semver
> Why reinvent the wheel? Semver was made to unify versions.

Well, semver doesn't work with between networked peers in a game environment because it is extremely difficult to maintain backwards compatibility between peers.  
If peer A is on 1.0.0, and peer B is on 1.1.0 (which relies on peer API to implement the feature), then peer B must fall back to 1.0.0. If the server reverted to client, then the session would be the lowest common version. Therefore, it would need to be the client that reverts version. Maintaining several server APIs per client would be very annoying already, but it would also introduce bugs. To fix the bugs, a 1.1.1 client-only patch would need to be released, which would fix 1.1.0 client's compatibility with 1.0.X server.  
On top of that that, a client can join a server as long as the client version is greater than the server version. This means that every release must contain a client that is compatible with every server release at that release time.

> Is this a replacement for semver?

For this project, yes. INL very good for end-user code, but not libraries.  

For libraries, I'd suggest using semver (to represent API), but adding an INL version to it. If your library is mid-connection only, omit I. Local may seme redundant to have with semver, but remember that semver revision represents bug fixes, some of which may not change the world state at all.  
Semver supports embedded other information inside of it, so you could use `major.minor.patch+inl.invite.0+inl.network.4+inl.local.3` (corresponds with INL `0.4.3`) here without maintaining another version.
