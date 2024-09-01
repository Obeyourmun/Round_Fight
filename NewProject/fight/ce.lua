function printValues(...)
    local args = {...}  -- 将可变参数存储在一个表中
    for i, v in ipairs(args) do
        print("Value " .. i .. ": " .. v)
    end

    print(args[5])
end

--printValues(1, "hello", 12, 3.14)



