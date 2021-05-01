<?php
namespace App\Models;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Factories\HasFactory;
class File extends Model
{
    use HasFactory;
    public function ApprovedBy(){
        return $this->belongsTo(User::class,'approvedBy_id');
    }
}
