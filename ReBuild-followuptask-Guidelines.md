# ReBuild followuptask 修复指南

## 1. 缺失功能修复计划

### 1.1 查看按钮弹出框缺失内容
**现状诊断**：`FollowUp/Components/Modules/FollowUpManagement/Dialogs/TaskDetailDialog.razor` 仅展示患者基础信息与时间轴，没有集成表单内容、出院证明预览以及随访结果入口，用户无法在一个弹窗内完成任务审阅；与旧实现 `F:\project\NTCare\NTCare\Components\Pages\Followup\SimpleCustomDialog.razor` 的完整信息面板相比存在明显功能缺口。

**修复步骤**：
1. **扩充数据模型**：在 `TaskDetailDialog` 中新增对 `patient_event`、`followup_record`、`followup_file_list` 的查询，并保存 `PatientEventId`、`FollowupRecordId`，以便下游组件复用。查询逻辑可参考旧服务 `F:\project\NTCare\NTCare\Services\TaskManagerService.cs` 中的 `CreateFollowupTaskModel`。
2. **嵌入表单展示**：直接在弹窗主体引入 `FormDataDisplay`，传入 `PatientEventId` 和 `Source="Followup"`，实现与旧系统一致的表单原样渲染，而不再依赖当前额外的“查看表单数据”对话框。
3. **补齐出院证明**：新增 `TaskAttachmentService`（位于 `FollowUp/Components/Modules/FollowUpManagement/Services`），实现 `ViewDischargeRecordAsync(Guid patientEventId)`，逻辑直接移植旧版 `TaskManagerService.ViewDischargeRecord`，输出列表交由已有的 `ImagePreviewDialog` 弹出。`TaskDetailDialog` 中在头部按钮区调用该服务。
4. **补齐随访结果**：把 `F:\project\NTCare\NTCare\Components\Pages\Followup\TaskResultView.razor` 迁移到本模块下（保持 BasePage 继承），提供 `FollowupRecordId` 参数；在任务状态为“已随访”时展示“查看随访结果”按钮并弹出该对话框，复现旧体验。
5. **统一交互入口**：更新 `FollowUp/Components/Modules/FollowUpManagement/Pages/TaskManager.razor` 中的 `HandleViewDetail`，确保始终打开强化后的对话框，并在关闭后根据需要刷新表格。

### 1.2 审核按钮弹框缺失功能
**现状诊断**：当前 `Dialogs/TaskAuditDialog.razor` 只有基础信息、备注输入和“直接通过”复选框，缺少表单预览、表单编辑、宣教选择、一键生成审核意见等能力；也无法让用户确认“可不填审核意见”；与旧有 `SimpleCustomDialog.razor` + `ApproveConfirmationDialog.razor` + `EducationSelectionDialog.razor` 的串联交互相比差距较大。

**修复步骤**：
1. **对话框结构加固**：在 `TaskAuditDialog` 中复用 1.1 步骤获得的完整任务数据，加入 `FormDataDisplay` 和“查看出院证明”按钮，让审核人能直接核对原始填写。
2. **恢复编辑能力**：移植 `F:\project\NTCare\NTCare\Components\Pages\Followup\FormSetEditor.razor`，在审核弹框中提供“编辑表单”按钮（仅审核权限可见），提交成功后重新加载 `FormDataDisplay`。
3. **宣教/审核流程**：把旧版 `ApproveConfirmationDialog.razor` 与 `EducationSelectionDialog.razor` 复制到 `FollowUp/Components/Modules/FollowUpManagement/Dialogs`，并在 `TaskAuditDialog.Submit` 中改为弹出多阶段对话框：
   - 第一步（ApproveConfirmation）允许填写审核意见、勾选“确认不填写审核意见直接通过”、触发“一键生成”逻辑（沿用旧代码中的 FormSetServiceFactory + `inj_zhiPuQingYanService` AI 接口）。
   - 如果需要宣教，则根据对话框返回的 `Signal` 打开 `EducationSelectionDialog`，完成宣教内容指定。
   - 对话框最终返回审核意见、`SkipMessage` 与 `followup_education_id`，由 `TaskAuditService.ApproveTaskAsync` 统一落库。
4. **移除多余拒绝按钮**：保持 `TaskAuditDialog` 仅有“通过”主流程，如需驳回使用单独入口，避免重复实现。
5. **刷新机制**：审核成功后统一调用表格刷新与统计刷新，行为与旧 `OnAuditApproved` 回调一致。
1. 

### 1.3 缺失的“添加随访任务”能力
**现状诊断**：新版工具栏只有“添加任务”按钮，对应的 `Dialogs/AddTaskDialog.razor` 只能针对单个患者和模板创建基础任务，无法批量建任务、设置访视期或选择既有表单集，严重缩水；页面下方也缺少旧版 `TaskManager.razor` 中的底栏入口，导致随访组长无法在 Multiselect 模式和“添加随访任务”之间切换。

**修复步骤**：
1. **恢复底部操作区**：参考 `F:\project\NTCare\NTCare\Components\Pages\Followup\TaskManager.razor` 中的 `MudPaper` 逻辑，在新页面加入“批量更换审核人/批量取消任务/添加随访任务”按钮区（受 `MultiSelection`、`IsCancelMode` 状态控制），确保管理员和组长使用路径一致；与此同时补充 `_selectedItems`、`MultiSelection` 等状态字段。
2. **复刻对话框能力**：把旧 `AddVisitTaskDialog.razor`（含患者筛选、访视期、表单集、审核人等字段）迁移到 `FollowUp/Components/Modules/FollowUpManagement/Dialogs`，保留 `SelectPatientsDialog.razor` 等依赖，实现多患者批量建任务；新增 `FixedPatientId` 参数以兼容后续扩展。
3. **扩展任务创建服务**：在 `TaskCreationService` 中增加“计划外/批量随访任务”创建方法，支持一次接收若干 `PatientModel` 并在单个事务里写入 `patient_event` 与 `followup_record`，逻辑可直接复用旧服务 `TaskManagerService` 中的 `Submit` 代码片段。
4. **统一按钮调用**：`TaskManager.razor` 中的“添加随访任务”按钮改为打开新的 `AddVisitTaskDialog`，保留原有 `HandleAddTask` 调用但注入“当前项目/当前用户/角色模式”等上下文，成功后刷新列表与统计。

## 2. 修复优先级与预估工作量
- **P1（高，约 2 人日）**：审核对话框能力补齐。直接影响“待审核”主流程，且需串联多个对话框与 AI 生成逻辑，优先落地。
- **P2（中，约 1.5 人日）**：查看对话框三项补完。实现方式与审核弹框高度复用，可在 P1 基础上快速完成。
- **P3（中，约 2 人日）**：添加随访任务入口与批量建任务。涉及 UI 状态与服务端事务，排在审阅链路之后实施。

### 2.1 进度状态
- 2025-11-20：P1 已完成（审核对话框功能补齐并通过编译）。
- 2025-11-21：P2 已完成（TaskDetailDialog 内嵌 FormDataDisplay、出院证明/随访结果入口；精简弹框仅保留基础身份信息；TaskManager 查看入口支持关闭后刷新）。
- 2025-11-21：P3 已完成（批量创建随访任务对话框/患者选择对话框落地，创建服务扩展；TaskManager 支持多选批量更换审核人或取消，添加随访任务独立入口；构建通过，净化警告后验证）。

## 3. 需要对比的原始实现文件
- `F:\project\NTCare\NTCare\Components\Pages\Followup\TaskManager.razor`
- `F:\project\NTCare\NTCare\Components\Pages\Followup\SimpleCustomDialog.razor`
- `F:\project\NTCare\NTCare\Components\Pages\Followup\ApproveConfirmationDialog.razor`
- `F:\project\NTCare\NTCare\Components\Pages\Followup\EducationSelectionDialog.razor`
- `F:\project\NTCare\NTCare\Components\Pages\Followup\TaskResultView.razor`
- `F:\project\NTCare\NTCare\Components\Pages\Followup\AddVisitTaskDialog.razor`
- `F:\project\NTCare\NTCare\Components\Pages\Followup\SelectPatientsDialog.razor`
- `F:\project\NTCare\NTCare\Components\Pages\Followup\FormSetEditor.razor`
- `F:\project\NTCare\NTCare\Services\TaskManagerService.cs`
