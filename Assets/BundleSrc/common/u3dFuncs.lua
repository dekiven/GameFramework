-- 说明：u3dFuncs
u3d = {}

function u3d.Instantiate(prefab)
    if nil ~= prefab then
        return UnityEngine.GameObject.Instantiate(prefab)
    end
end