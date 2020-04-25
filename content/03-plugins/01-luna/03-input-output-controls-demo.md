---
title: "输入输出控件演示"
date: 2020-02-26T13:22:12+08:00
draft: false
weight: 3
---

##### Alert, Choice, Input
```lua
function Main()
    local choices = {
        "显示当前时间(Alert演示)",
        "简单计算(Input演示)",
        "你喜欢什么水果(Choices演示)",
    }
        
    local idx = Misc:Choice("请选择演示内容(点'取消'退出)", choices, true)
    
    if idx == 1 then
        Misc:Alert(os.date('%Y-%m-%d %H:%M:%S'))
    end
    
    if idx == 2 then
        local expr = Misc:Input("请输入简单算式, 例如: 1+2*3")
        local f = load('return ' .. expr)
        Misc:Alert(tostring(f()))
    end
    
    if idx == 3 then
        ChooseFruit()
    end
    
    if idx < 1 then
        return false
    end
    return true
end

function ChooseFruit()
    local fruit = {
        "香蕉",
        "橙子",
        "鸭梨",
    }
    
    local choices = Misc:Choices("你喜欢吃什么水果?", fruit)
    local r = ""
    for index in Each(choices) do
        r = r .. fruit[index] .. ","
    end
    if string.isempty(r) then
        Misc:Alert("没一个喜欢的")
    else
        Misc:Alert(string.sub(r, 0, #r - 1))
    end
end

local again = true
while again do
    again = Main()
end
```

##### ShowData输出控件
```lua
-- Misc:ShowData() 示例

local Utils = require "libs.utils"

function Main()
    local selected = ShowAllServers()
    if selected ~= nil then
        ShowResult(selected)
    end
end

function ShowResult(rows)
    print("选中:")
    for row in Each(rows) do
        print(row[0] .. "." .. row[1])
    end
end

function ShowAllServers()
    local rows = {}
    for coreServ in Each(Server:GetAllServers()) do
        local coreState = coreServ:GetCoreStates()
        local row = {
            coreState:GetIndex(),
            coreState:GetLongName(),
            coreState:GetSummary(),
            coreState:GetMark(),
            Utils.ToLuaDate(coreState:GetLastModifiedUtcTicks()),
            coreState:GetStatus(),
        }
        table.insert(rows, row)
    end
    local columns = {"序号", "名称", "摘要", "标记", "修改日期", "测速"}
    return Misc:ShowData("服务器列表:", columns, rows, 3)
end

Main()
```
