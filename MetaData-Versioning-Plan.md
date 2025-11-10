# FormSet 元数据版本化方案

本文档旨在确保 FollowUp 表单元数据的所有变动都能被可靠追踪、回溯和审计。核心目标包括：

1. **变更追踪**：每日导出的快照应在 Git 中直观呈现所有结构变更，包括表单集、表单、卡片、问题的新增/删除，以及顺序或可见性调整。
2. **历史回溯**：支持对任意两个版本（commit）进行比对，快速定位差异点，方便业务与开发双向核查。
3. **自动化审计基础**：快照格式要稳定、易解析，为后续构建更友好的差异分析工具（如图形化 diff）提供可靠数据源。

实现约束：
- 使用最简文本格式表达表单集→表单→卡片→问题的全量层级关系与必要属性。
- 输出顺序必须可重复，顺序数据与数据库真实链表保持一致。顶层实体无链表时按 `sort_index`+`id` 固定排序。
- 仅记录当前需求涉及的字段，不额外抽象；所有内容使用 UTF-8 with BOM 编码，并遵循 YAGNI 原则。

## 1. 目标与范围
- 每日生成一次全量快照文件，覆盖组织层级（医院/科室/病区/项目）到表单集（FormSet）及其下属 Form、Card、Question 的全部定义。
- 快照必须能够反映双向链表定义的顺序变化、Card 嵌套层级、Card 与 Question 的混排关系，以及实体的新增/删除/停用。
- 输出文件需完全确定性：相同输入数据在任何时刻导出结果一致。

## 2. 文本格式：FSD v4（FormSet Snapshot Definition）

### 2.1 层级与实体
1. `HOSPITAL` → `DEPT` → `WARD` → `PROJECT` → `FORM_SET`
2. `FORM_SET` 下包含 `FORM`
3. `FORM` 和所有 `CARD` 节点通过单一链表记录子元素顺序，节点类型由 `SLOT.slot_type` 标识，可混合出现 `card` 与 `question`
4. `CARD` 节点支持多级嵌套，其子元素同样通过 `SLOT` 表示
5. `QUESTION` 节点只描述自身元数据，不再嵌套其他实体

### 2.2 顺序规则
- 顶层组织层级（`HOSPITAL` → `DEPT` → `WARD` → `PROJECT` → `FORM_SET` → `FORM`）在数据库中没有链表指针，导出时先按业务排序字段（如 `form_form.sort_index`），再按主键 `id` 升序输出，保证 diff 稳定。
- 只有 `form_card`、`form_question` 表包含链表指针列 `pre_uid`、`next_uid`。导出时必须严格依赖这两个字段重建顺序，文本输出行序与链表遍历一致。
- 若链表断裂或出现孤立节点，可按 `id` 升序收集剩余节点附加在尾部，并在 `WARN` 区域记录问题，便于后续修复。

### 2.3 字段清单（结合真实表结构）
- 通用字段：`id`、`name`、说明性字段（如 `description`、`group_name`）、以及派生状态（例如通过 `is_hidden` 推导 `status=inactive`）。
- `HOSPITAL` (`system.sys_hospital`)：`id`、`name`。
- `DEPT` (`system.sys_department`)：`id`、`hospital_id`、`name`、`display_name`。
- `WARD` (`system.sys_ward`)：`id`、`hospital_id`、`department_id`、`name`。
- `PROJECT` (`form.form_project`)：`id`、`ward_id`、`department_id`、`hospital_id`、`name`、`display_name`。
- `FORM_SET` (`form.form_form_set`)：`id`、`name`、`description`、`type`、`project_id`、`ward_id`、`department_id`、`hospital_id`。
- `FORM` (`form.form_form`)：`id`、`name`、`group_name`、`sort_index`、`is_hidden`、`form_set_id`。
- `CARD` (`form.form_card`)：`id`、`parent_id`、`type`、`name`、`parameter`、`form_id`、`pre_uid`、`next_uid`、`is_hidden`。`depth` 为导出时根据 `parent_id` 计算的派生字段。
- `QUESTION` (`form.form_question`)：`id`、`card_id`、`table_name`、`column_name`、`data_type`、`display_name`、`sort_index`、`is_required`、`is_hidden`、`pre_uid`、`next_uid` 等；其余配置字段可按需进入 `extra={...}`。
- `SLOT`：导出层的虚拟节点，用于表达 `FORM` 或 `CARD` 子元素的混排顺序。字段包括 `slot_type`（card/question）、`target_id`、`pre_uid`、`next_uid`、`card_id`（仅当父节点为 Card 时标识所属 Card）。该结构完全由导出脚本根据链表信息生成，可采用内部生成的 `slot-{序号}` 作为引用 id。
- 派生状态：`status` 可根据 `is_hidden` 或业务标识生成，例如 `status=inactive` 对应 `is_hidden=true`。
- 扩展信息（选项集、默认值、尺寸参数等）可放入 `extra={...}` JSON，键按字母序排列。

### 2.4 输出示例
```plaintext
# FormSet Snapshot v4
# generated_at=2025-10-19T17:00:00Z

HOSPITAL id="hosp-01" name="北京协和医院" status="active"
    DEPT id="dept-A" parent_id="hosp-01" name="内科" status="active"
        WARD id="ward-101" parent_id="dept-A" name="呼吸内科一病区" status="active"
            PROJECT id="proj-lc" parent_id="ward-101" name="肺癌随访项目" status="active"
                FORM_SET id="fs-001" parent_id="proj-lc" name="肺癌随访表-V2" version="2024-05-01" status="active"
                    FORM id="f-01" parent_id="fs-001" name="首次随访" sort_index=10
                        SLOT slot_type="card" target_id="c-101" pre_uid=null next_uid="slot-011"
                        SLOT slot_type="question" target_id="q-1002" pre_uid="slot-010" next_uid=null
                    FORM id="f-02" parent_id="fs-001" name="复诊记录" sort_index=20

CARD id="c-101" parent_card_id=null depth=0 type="default" sort_index=10 pre_uid=null next_uid="c-102"
    SLOT slot_type="question" target_id="q-1002" pre_uid=null next_uid="slot-010020"
    SLOT slot_type="card" target_id="c-201" pre_uid="slot-010010" next_uid=null

CARD id="c-201" parent_card_id="c-101" depth=1 type="multiple" sort_index=10 pre_uid=null next_uid=null
    SLOT slot_type="question" target_id="q-2001" pre_uid=null next_uid=null

QUESTION id="q-1002" parent_card_id="c-101" table_name="form.patient_basic" column_name="diagnosis_date"
         data_type="date" is_required=true pre_uid=null next_uid="q-1003"
QUESTION id="q-2001" parent_card_id="c-201" table_name="form.patient_detail" column_name="exam_result"
         data_type="text" is_required=false pre_uid=null next_uid=null

WARN missing_link card_id="c-101" reason="next_uid 指向不存在的节点，已按 id 补排"
```
- 当实体被删除时，其对应块在 Git diff 中会被整体删除；若仅停用，则将 `status` 设置为 `inactive`。
- `WARN` 区域可记录链表异常、缺失节点等问题，便于人工跟进。

### 2.5 版本管理收益
- **变更追踪**：通过 Git diff 可直接看到新增/删除的行、`pre_uid`/`next_uid` 的调整，从而定位问题、卡片、表单顺序的变更。
- **历史回溯**：任意两个快照文件（commit）之间的差异比较即可还原当时的表单结构。
- **自动化审计**：格式稳定、字段清晰，后续可编写解析器或可视化工具，在不改变文件结构的前提下提供更友好的差异视图。

## 3. 导出流程

1. **数据提取**  
   - 编写 Python 脚本使用 `psycopg` 直连数据库，执行预定义 SQL 查询拉取 `form_form_set`、`form_form`、`form_card`、`form_question` 及组织架构相关表（`sys_hospital`、`sys_department`、`sys_ward`、`form_project`）的数据。  
   - 查询语句仅选择必要列：顶层实体取 `id`、`name` 及上级关联字段；`form_form` 额外取 `group_name`、`sort_index`、`is_hidden`；`form_card` 取 `parent_id`、`type`、`name`、`parameter`、`form_id`、`pre_uid`、`next_uid`、`is_hidden`；`form_question` 取 `card_id`、`table_name`、`column_name`、`data_type`、`display_name`、`sort_index`、`is_required`、`is_hidden`、`pre_uid`、`next_uid`。  
   - 所有查询显式按主键 `id` 升序排序，避免底层顺序抖动。

2. **顺序重建**  
   - 顶层组织层级按 `id`（必要时先按 `sort_index`）排序生成输出序列。  
   - `CARD`/`QUESTION` 层级建立 `id → 节点` 字典，并根据 `pre_uid` / `next_uid` 找到链表头。  
   - 按链表顺序遍历节点；链表断裂时按 `id` 升序补全，并在 `WARN` 中写明原因。  
   - 对 `CARD` 与 `QUESTION` 的混排，构建 `SLOT` 集合，按照 `pre_uid` / `next_uid` 顺序输出即可得到最终排列。

3. **序列化**  
   - 严格按层级与链表遍历顺序输出节点，缩进使用 4 个空格。  
   - 字段顺序固定：标识→顺序→状态→扩展。缺失值统一写 `null`。  
   - `extra` JSON 键按字母序排列，布尔值写成 `true`/`false`。

4. **文件落地与一致性校验**  
   - 导出前清理旧文件，写入新内容（UTF-8 with BOM）。  
   - 生成后再次遍历校验：  
     - 所有 `pre_uid`/`next_uid` 是否成对；  
     - 是否存在断链、孤立节点或循环引用，并记录到 `WARN`；  
     - `SLOT` 指向的实体是否存在。  
   - 若校验失败，输出 `WARN` 并返回非零退出码，避免提交错误快照。

5. **Git 提交**  
   - Python 脚本调用 `subprocess` 执行 `git status --porcelain` 检查差异，仅在快照文件发生变更时执行 `git add/commit`，提交信息统一为 `[Auto] FormSet snapshot YYYY-MM-DD`。  
   - 调度前确保工作目录干净，避免混入其他改动。

### 3.1 关键算法参考

> 以下示例假定输入数据已预处理出唯一的 `uid`、稳定的 `id` 以及所属父容器标识 `container_uid`，并按父容器拆分后逐一调用。

#### 链表顺序重建（Card / Question）

```python
WARNINGS = []

def rebuild_linked_sequence(rows):
    """rows 需包含 uid、pre_uid、next_uid、id 字段"""
    if not rows:
        return []

    heads = [row for row in rows if row.get("pre_uid") is None]
    if len(heads) > 1:
        WARNINGS.append({"code": "multi_head", "uids": [row["uid"] for row in heads]})
    if heads:
        head = min(heads, key=lambda r: r["id"])
    else:
        head = min(rows, key=lambda r: r["id"])
        WARNINGS.append({"code": "head_missing", "fallback": head["uid"]})

    remaining = {row["uid"]: row for row in rows}
    sequence, seen = [], set()

    while head:
        uid = head["uid"]
        if uid in seen:
            WARNINGS.append({"code": "cycle", "at": uid})
            break
        sequence.append(head)
        seen.add(uid)
        remaining.pop(uid, None)

        next_uid = head.get("next_uid")
        if next_uid == uid:
            WARNINGS.append({"code": "self_loop", "uid": uid})
            break
        if next_uid in seen:
            WARNINGS.append({"code": "cycle", "at": next_uid})
            break

        head = remaining.get(next_uid)

    dangling = sorted(
        [row for uid, row in remaining.items() if uid not in seen],
        key=lambda r: r["id"]
    )
    if dangling:
        WARNINGS.append({"code": "chain_gap", "ids": [row["uid"] for row in dangling]})
    return sequence + dangling
```

#### `SLOT` 混排构建

```python
def build_slots(container_uid, card_rows, question_rows):
    def ensure_container(rows, kind):
        mismatched = [row["uid"] for row in rows if row.get("container_uid") != container_uid]
        if mismatched:
            WARNINGS.append({
                "code": "parent_mismatch",
                "kind": kind,
                "uids": mismatched,
                "expected": container_uid
            })

    ensure_container(card_rows, "card")
    ensure_container(question_rows, "question")

    items = [
        {
            "uid": row["uid"],
            "pre_uid": row.get("pre_uid"),
            "next_uid": row.get("next_uid"),
            "slot_type": "card",
            "target_id": row["id"],
            "id": row["id"]
        }
        for row in card_rows
    ] + [
        {
            "uid": row["uid"],
            "pre_uid": row.get("pre_uid"),
            "next_uid": row.get("next_uid"),
            "slot_type": "question",
            "target_id": row["id"],
            "id": row["id"]
        }
        for row in question_rows
    ]

    ordered = rebuild_linked_sequence(items)
    for index, item in enumerate(ordered):
        slot_id = f"slot-{index:04d}"
        yield {
            "slot_id": slot_id,
            "slot_type": item["slot_type"],
            "target_id": item["target_id"],
            "container_uid": container_uid
        }
```

#### 文件写入与一致性校验

```python
from pathlib import Path

def write_snapshot(path: Path, lines):
    content = "\n".join(lines) + "\n"
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8-sig")

def assert_consistency(container_uid, slots, cards_by_id, questions_by_id):
    missing = []
    for slot in slots:
        if slot["slot_type"] == "card":
            card = cards_by_id.get(slot["target_id"])
            if card is None:
                missing.append(slot)
                continue
            if card.get("container_uid") != container_uid:
                WARNINGS.append({
                    "code": "card_parent_mismatch",
                    "slot_id": slot["slot_id"],
                    "target_id": slot["target_id"],
                    "expected": container_uid,
                    "actual": card.get("container_uid")
                })
        else:
            question = questions_by_id.get(slot["target_id"])
            if question is None:
                missing.append(slot)
                continue
            if question.get("container_uid") != container_uid:
                WARNINGS.append({
                    "code": "question_parent_mismatch",
                    "slot_id": slot["slot_id"],
                    "target_id": slot["target_id"],
                    "expected": container_uid,
                    "actual": question.get("container_uid")
                })
    if missing:
        WARNINGS.append({"code": "missing_target", "slots": missing})
    return not WARNINGS
```

## 4. 实现建议（保持最小改动）

1. **导出脚本**  
   - 在 `scripts` 目录新增 `export_formset_snapshot.py`，依赖 Python 3.11+ 与 `psycopg`、`python-dotenv`（可选）等轻量库。  
   - 模块结构建议：`load_data()`、`build_snapshot()`、`write_file()`、`run_checks()`，组合实现“加载数据 → 构建顺序 → 输出文本 → 返回校验结果”。

2. **命令入口**  
   - 提供命令行入口：`python scripts/export_formset_snapshot.py --conn "..." --output snapshots/.../formset.fsd --repo-root ..`。  
   - 使用 `argparse` 解析参数，支持从环境变量读取连接串、输出目录、Git 用户设置。

3. **计划任务集成**  
   - Windmill（或其他计划任务）执行步骤：  
     1. 拉取仓库最新代码并安装 Python 依赖（`pip install -r scripts/requirements.txt`）。  
     2. 执行导出脚本生成快照。  
     3. 若脚本返回成功且存在文件变更，则执行 `git commit` 并推送。  
   - 调度环境需预置 Git 凭据、数据库连接信息等敏感配置，可使用 Windmill 的密钥管理。

## 5. 后续工作
- [ ] 明确快照输出目录与文件命名规范（如 `snapshots/{project_code}/formset_{date}.fsd`）。  
- [ ] 定义 `extra` 字段需要覆盖的最小集合（如选项集、默认值、校验规则）。  
- [ ] 为 Python 脚本补充最小的自检日志，便于排查链表异常。  
- [ ] 与业务确认停用状态、逻辑删除的表示方式，避免误判 diff。

该方案在不增加额外项目的前提下，实现对 FormSet 结构的完整、稳定快照，便于后续使用 Git 工具快速定位任何层级的结构变更。
