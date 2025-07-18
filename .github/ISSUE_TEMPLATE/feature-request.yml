name: "Request content or suggest an idea / Запросить контент или предложить идею"
description: "You can suggest new content, mechanics or changes to existing systems. / Предложите контент, механику или улучшение."
labels: [suggestion]
type: Suggestion
body:
  - type: markdown
    attributes:
      value: |
        ## 🧠 Concept / Концепция

        Describe your idea or proposal in a concise way. What's the core of it?

        Опишите вашу идею кратко. В чём её суть?

  - type: textarea
    id: concept
    attributes:
      label: Concept / Концепция
      description: |
        What is the basic idea? What problem does it solve, or what new interaction does it create?
        В чём заключается идея? Какую проблему она решает или какую новую механику создаёт?

      placeholder: |
        Example / Пример: Add a weather system that affects player visibility and movement speed.
    validations:
      required: true

  - type: textarea
    id: motivation
    attributes:
      label: Motivation / Мотивация
      description: |
        Why is this needed? What’s currently lacking or could be better?
        Почему это важно? Чего сейчас не хватает или что можно улучшить?

      placeholder: |
        Example / Пример: Exploration is too repetitive and weather would make it more dynamic.
    validations:
      required: false

  - type: textarea
    id: implementation
    attributes:
      label: Implementation ideas / Идеи по реализации
      description: |
        How do you imagine it could be implemented mechanically or technically?
        Как вы представляете себе реализацию этой идеи (механики, системы, события)?

      placeholder: |
        Example / Пример: Weather zones could randomly appear on the map and modify player stats.
    validations:
      required: false

  - type: textarea
    id: visuals
    attributes:
      label: Visuals / Изображения, схемы
      description: |
        Add concept art, diagrams, or mockups if available. These can help others understand your idea better.
        Прикрепите схемы, мокапы, арты или другие изображения, помогающие понять суть идеи.

      placeholder: |
        Example / Пример: 
        - Diagram of new UI element
        - Map sketch showing proposed zone behavior
    validations:
      required: false

  - type: textarea
    id: concerns
    attributes:
      label: Risks / Возможные проблемы
      description: |
        Are there any possible issues, edge cases, or balance concerns that come with this suggestion?
        Какие сложности или риски могут возникнуть при реализации этой идеи? Есть ли проблемы баланса?

      placeholder: |
        Example / Пример: Players might abuse the mechanic if it stacks with speed boosts.
    validations:
      required: false

  - type: input
    id: related
    attributes:
      label: Related content / Связанные идеи
      description: |
        Is this idea building on something that already exists or was proposed earlier?
        Эта идея связана с существующим контентом или другими предложениями?
      placeholder: Issue #123 or "Cooking system rework"
    validations:
      required: false
