

**1st** **SIT** **Coursework** **-** **01** **Question** **paper**
**Year** **Long** **2025** **2026**

> **Module** **Code:**
>
> **Module** **Title:**
>
> **Module** **Leader:**

**CS6004NI**

**Application** **Development**

**Mr.** **Bikram** **Poudel** (Islington College)

||
||
||
||
||
||
||
||
||

> © London Metropolitan University

> **SCENARIO**

A writer wants to modernize their personal journaling process with a
**secure,** **feature-rich** **desktop** **application**. The system
must allow the user to **create,** **update,** **and** **delete**
**one** **journal** **entry** **per** **day**, supporting **rich-text**
**or** **Markdown** **formatting** for content. Each entry is tied to a
**date** **and** **time**, with system-generated timestamps for creation
and updates.

Users should be able to **track** **their** **moods**, selecting **one**
**primary** **mood** and up to **two** **secondary** **moods**,
categorized as **Positive,** **Neutral,** **or** **Negative**.
Additionally, users can **tag** **entries** with custom or pre-defined
tags (e.g., Work, Health, Travel, Fitness) and organize entries under
categories.

The system must provide easy navigation of past entries through a
calendar view and a

paginated timeline/list view. Users should be able to search entries by
title or content and filter by date range, mood(s), or tags.

To encourage consistent journaling, the application will track **daily**
**streaks,** **longest**

**streaks,** **and** **missed** **days**. A **dashboard** will provide
analytics, including mood distribution, most frequent moods, most used
tags, tag breakdown, and word count trends over time.

The application must also support **security** **and** **privacy** via
password or PIN protection,

allow **exporting** **journals** **as** **PDF** by date range, and store
all data **locally** **in** **an** **SQLite** **database**. Optional
features include **theme** **customization** **(light/dark)** for a
personalized interface.

**Journal** **Structure** • **Rich-text/Markdown** **Content**

• **DateTime** → Created At, Updated At (System-generated) •
**Mood(s):**

> o **Primary** **Mood** **(Required):** One main mood for analytics o
> **Secondary** **Mood** **(Optional):** Up to two additional moods o
> Categories:
>
> ▪
>
> ▪
>
> ▪

• **Category**

**Positive:** Happy, Excited, Relaxed, Grateful, Confident

**Neutral:** Calm, Thoughtful, Curious, Nostalgic, Bored

**Negative:** Sad, Angry, Stressed, Lonely, Anxious

• **Tags** **(Optional):**

> o **Custom** **Tags**
>
> o **Pre-built** **Tags:** Work, Career, Studies, Family, Friends,
> Relationships, Health, Fitness, Personal Growth, Self-care, Hobbies,
> Travel, Nature, Finance, Spirituality, Birthday, Holiday, Vacation,
> Celebration, Exercise, Reading, Writing, Cooking, Meditation, Yoga,
> Music, Shopping, Parenting, Projects, Planning, Reflection

• **One** **Journal** **Entry** **per** **Day** → Supports daily streaks

• **CRUD** **Operations** → Create, update, or delete the entry for the
current day • **Search** **&** **Filter** → By content, date range,
moods, or tags

• **Analytics** **(Date** **Range** **Filterable)**

> o **Mood** **Distribution** → Pie/Bar chart showing % of positive,
> neutral, negative moods
>
> o **Most** **Frequent** **Mood** → The most common mood recorded o
> **Daily** **Streak** → Current consecutive days of journaling
>
> o **Longest** **Streak** → Maximum streak achieved o **Missed**
> **Days** → Dates with no entries
>
> o **Most** **Used** **Tags** → Bar chart/word cloud of frequent tags
>
> o **Tag** **Breakdown** → % of entries per category (e.g., Work,
> Health, Travel) o **Word** **Count** **Trends** → Average words per
> entry over time
>
> **TASK**

The deliverables are outlined on the first page. Review the provided
scenario and analyze it to extract all relevant requirements. For
reference, the fixed requirements are detailed in the marking scheme
below.

You are tasked with developing a **C#.NET** **software** **application**
based on the given scenario. Additionally, clarify your contribution
according to the project type (i.e., individual or group).

After completing the application, ensure it is thoroughly documented.
Refer to the marking scheme for documentation requirements and review
the "Things to Remember" section for further guidance.

> **THINGS** **TO** **REMEMBER**

1\. Plagiarism:

> a\. Plagiarism is grounds for failure and applies to deliverables. All
> parties involved will be penalized.
>
> b\. Code-level plagiarism is prohibited. Always provide proper
> attribution for any borrowed code.

2\. Documentation:

> a\. Development without accompanying documentation (and vice versa) is
> not allowed and will result in failure.

3\. Group Projects:

> a\. Groups can have up to 6 members. Only the group leader is
> responsible for submission.

4\. Frameworks:

a\. Any framework under C#.NET is permitted. 5. Deployment:

a\. Deploying or publishing the completed application is encouraged. 6.
Architectural Patterns:

a\. Following well-known architectural patterns is encouraged. 7.
Non-Functional Requirements:

> a\. Addressing non-functional requirements such as security,
> performance, scalability, compatibility, and usability will enhance
> your project’s evaluation.

8\. Development Tools:

> a\. Using project templates, libraries, packages, or modules to
> simplify the development process is encouraged, provided you justify
> their use.

9\. Version Control:

> a\. Use version control systems (e.g., Git) to manage your codebase.
> Regular commits and clear commit messages are important.

10\. User Experience (UX):

> a\. Pay attention to the user interface (UI) and user experience (UX)
> design. An intuitive and user-friendly application will positively
> impact your evaluation.

11\. Project Management:

> a\. Demonstrate effective project management practices, including
> setting milestones, tracking progress, and managing resources. This
> will be reflected in your project review.

12\. References:

> a\. Always provide references for any external resources used, such as
> libraries, remote repositories, articles, forums, or Q&A sites.
>
> ***"We*** ***welcome*** ***and*** ***encourage*** ***your***
> ***creativity."***
>
> **MARKING** **SCHEME**

||
||
||
||
||
||
||
||
||
||
||
||
||
||
||
||
||
||

||
||
||
||
||
||
||
||
||
||
||
||
||
||
||
||

> **MILESTONE** **1** **(Week** **9** **(December** **21,** **2025))**

1\) **Task** **1:** Initialize private Git repository with an empty MAUI
project.

2\) **Task** **2:** **Project** **Report**

> a\) Project Overview
>
> b\) UI Design (Wireframe)
>
> c\) Data/Entity Modelling (E.g. Mood, Entry, User etc.)
>
> d\) Technology Stack
>
> i\) **Framework** (E.g. MAUI Blazor Hybrid, WinForms, Xamarin etc.)
>
> ii\) **External** **Libraries** (E.g. Newtonsoft.Json, Bogus,
> MudBlazor etc.)
>
> iii\) **Persistence** **Mechanism**: (e.g., file handling using local
> database SQLite)

3\) **Task** **3:** Must complete at least 4 features.
