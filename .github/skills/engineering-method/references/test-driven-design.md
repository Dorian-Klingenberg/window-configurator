# Test-driven design

The working idea is to use the expression and observation of desired behavior to inform design. It is not limited to a mechanical test-first coding loop. A scenario, example, proposed test, or executable test can reveal questions appropriate to the current stage.

1. Describe a meaningful behavior and the observable outcome.
2. Consider a natural way to exercise and observe it.
3. Identify concrete friction: hidden time, uncontrollable dependencies, difficult setup, private-state access, or coupling, for example.
4. Ask whether the cause lies in the requirement, the test, the environment, or the production design. Friction is a clue, not proof of an architectural defect.
5. Discuss the smallest design response justified by evidence, including keeping the existing design.
6. When implementation is authorized, verify behavior with appropriate tests and investigate surprising results.

Do not invent abstractions simply because they are conventional. Do not turn exploratory examples into accepted requirements, or require executable tests for every discovery conversation. An illustrative test may introduce a particular distance, object identifier, collision rule, or storage fake to make an example concrete. Label those as assumptions or options unless the user or project has established them. An observable behavior such as recognizing a moved chair does not by itself mandate a particular identity representation or persistence design.
